# Copyright (c) 2023 Qualcomm Technologies, Inc.
# All rights reserved.
import numpy as np
import torch
from gatr.experiments.base_wrapper import BaseWrapper
from gatr.interface import (
    embed_point,
    embed_scalar,
    embed_translation,
    extract_point,
    extract_point_embedding_reg,
)

def embed_nbody_data_in_pga(inputs):
    """Represent the n-body initial state in PGA multivectors.

    Masses are represented as scalars, positions as trivectors, and velocities as bivectors
    (like translations).  All three are summed (this is equivalent to concatenation, as an equi
    linear layer can easily separate the grades again).

    This function is used both by the GATr and by the GCAN wrappers.

    Parameters
    ----------
    inputs : torch.Tensor with shape (batchsize, objects, 7)
        n-body initial state: a concatenation of masses, initial positions, and initial
        velocities along the feature dimension.

    Returns
    -------
    multivector : torch.Tensor with shape (batchsize, objects, 1, 16)
        GA embedding.
    """

    # Build one multivector holding masses, points, and velocities for each object
    masses = inputs[:, :, [0]]  # (batchsize, objects, 1)
    masses = embed_scalar(masses)  # (batchsize, objects, 16)
    points = inputs[:, :, 1:4]  # (batchsize, objects, 3)
    points = embed_point(points)  # (batchsize, objects, 16)
    velocities = inputs[:, :, 4:7]  # (batchsize, objects, 3)
    velocities = embed_translation(velocities)  # (batchsize, objects, 16)
    multivector = masses + points + velocities  # (batchsize, objects, 16)

    # Insert channel dimension
    multivector = multivector.unsqueeze(2)  # (batchsize, objects, 1, 16)

    return multivector


class NBodyGATrWrapper(BaseWrapper):
    """Wraps around GATr for the n-body prediction experiment.

    Parameters
    ----------
    net : torch.nn.Module
        GATr model that accepts inputs with 1 multivector channel and 1 scalar channel, and
        returns outputs with 1 multivector channel and 1 scalar channel.
    """

    def __init__(self, net):
        super().__init__(net, scalars=True, return_other=True)
        self.supports_variable_items = True

    def embed_into_ga(self, inputs):
        """Embeds raw inputs into the geometric algebra (+ scalar) representation.

        Parameters
        ----------
        inputs : torch.Tensor with shape (batchsize, objects, 7)
            n-body initial state: a concatenation of masses, initial positions, and initial
            velocities along the feature dimension.

        Returns
        -------
        mv_inputs : torch.Tensor
            Multivector representation of masses, positions, and velocities.
        scalar_inputs : torch.Tensor or None
            Dummy auxiliary scalars, containing no information.
        """
        batchsize, num_objects, _ = inputs.shape

        # Build one multivector holding masses, positions, and velocities for each object
        multivector = embed_nbody_data_in_pga(inputs)

        # Scalar inputs are not really needed here
        scalars = torch.zeros((batchsize, num_objects, 1), device=inputs.device)

        return multivector, scalars

    def extract_from_ga(self, multivector, scalars):
        """Extracts raw outputs from the GATr multivector + scalar outputs.

        We parameterize the predicted final positions as points.

        Parameters
        ----------
        multivector : torch.Tensor
            Multivector outputs from GATr.
        scalars : torch.Tensor or None
            Scalar outputs from GATr.

        Returns
        -------
        outputs : torch.Tensor
            Predicted final-state positions.
        other : torch.Tensor
            Regularization terms.
        """

        # Check channels of inputs. Batchsize and object numbers are free.
        assert multivector.shape[2:] == (1, 16)
        assert scalars.shape[2:] == (1,)

        # Extract position
        points = extract_point(multivector[:, :, 0, :])

        # Extract non-point components and compute regularization
        other = extract_point_embedding_reg(multivector[:, :, 0, :])
        reg = torch.sum(other**2, dim=[1, 2])
        if self.scalars:
            reg = reg + torch.sum(scalars**2, dim=[1, 2])

        return points, reg
