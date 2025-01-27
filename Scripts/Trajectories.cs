using System.Collections.Generic;
using Unity.Engine;
using Unity.Sentis;

class Trajectories : MonoBehaviour
{

    const int timeSteps = 100; 
    const double deltaT = 0.1;

    public static (Tensor finalPositions, Tensor trajectory) Simulate(Tensor initialState)
    {
        int batchSize = initialState.shape[0];  // Always 1 in your case
        int numObjects = initialState.shape[1];

        // Extract masses, positions, and velocities from the tensor
        Tensor masses = initialState.Select(axis: 2, index: 0); // Select the mass (first element in the 7)
        Tensor positions = initialState.Select(axis: 2, index: new[] { 1, 2, 3 }); // Select positions (x, y, z)
        Tensor velocities = initialState.Select(axis: 2, index: new[] { 4, 5, 6 }); // Select velocities (vx, vy, vz)

        // List to store trajectory
        List<Tensor> trajectory = new List<Tensor> { positions };

        // Simulate for the given number of time steps
        for (int t = 0; t < timeSteps; t++)
        {
            // Compute accelerations based on positions
            Tensor accelerations = ComputeAccelerations(masses, positions);

            // Update velocities and positions
            velocities = velocities + deltaT * accelerations;
            positions = positions + deltaT * velocities;

            // Append the new positions to the trajectory
            trajectory.Add(positions);
        }

        // Convert trajectory to tensor and transpose
        Tensor trajectoryTensor = Tensor.Concat(trajectory.ToArray(), axis: 3); // Concatenate along the time axis
        trajectoryTensor = trajectoryTensor.Transpose(0, 1, 3, 2); // Transpose to match shape [batch, num_objects, time_steps, 3]

        // Final positions after the simulation
        return (positions, trajectoryTensor);
    }

    private Tensor ComputeAccelerations(List<double> masses, Tensor positions)
    {
        int batchSize = positions.shape[0];
        int numObjects = positions.shape[1];

        // Reshape masses into tensors for pairwise computation
        Tensor m1 = new Tensor(batchSize, 1, numObjects, 1, masses.ToArray());
        Tensor m2 = new Tensor(batchSize, numObjects, 1, 1, masses.ToArray());

        // Compute pairwise distance vectors
        Tensor pos1 = positions.Reshape(batchSize, 1, numObjects, 3);
        Tensor pos2 = positions.Reshape(batchSize, numObjects, 1, 3);
        Tensor distanceVectors = pos1 - pos2;

        // Compute distances (norm of vectors)
        Tensor distances = distanceVectors.ReduceL2(axes: -1).Unsqueeze(-1);

        // Avoid division by zero
        distances.Assign(distances, d => Mathf.Max((float)d, 1e-9f));

        // Compute forces
        Tensor distancesCubed = distances.Pow(3);
        Tensor forces = (distanceVectors * (m1 * m2)) / distancesCubed;

        // Sum forces to compute accelerations
        Tensor summedForces = forces.ReduceSum(axis: 2);
        Tensor massesTensor = new Tensor(batchSize, numObjects, 1, masses.ToArray());
        Tensor accelerations = summedForces / massesTensor;

        // Cleanup and return
        m1.Dispose();
        m2.Dispose();
        pos1.Dispose();
        pos2.Dispose();
        distanceVectors.Dispose();
        distances.Dispose();
        distancesCubed.Dispose();
        summedForces.Dispose();
        massesTensor.Dispose();

        return accelerations;
    }
}
