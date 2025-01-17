## GETTING STARTED
Start by cloning the repository using:
`git clone https://github.com/Qualcomm-AI-research/geometric-algebra-trasformer`.
Open the cloned repository and from here build the Docker image.
On Linux-like systems this can be done running the following commands.
```bash
cd geometric-algebra-transformer
docker build -f docker/Dockerfile --tag gatr:latest .
```
Once built the image can be run via:
`sudo docker run --rm -it -v $PWD:$PWD -w $PWD --memory=8g gatr:latest /bin/bash`
which also exposes the host's GPUs to the container.

## Running experiments

First, we need to generate training and evaluation datasets:
```bash
python scripts/generate_nbody_dataset.py
```

Let's train a GATr model, using just 1000 (or 1%) training trajectories and training for 5000 steps:

```bash
python scripts/nbody_experiment.py model=gatr_nbody data.subsample=0.01 training.steps=5000 run_name=gatr
```
## Prediction Example

#### Input tensor of dimension [1, 4, 7] (1 sample for 4 planets with 7 parameters)

Initial positions (x, y, z) of planets:
```bash 
[[-19.30458052  -5.09142425  -1.71743116]
 [-18.95293991  -4.954115    -2.10290605]
 [-18.71318257  -4.56528173  -2.06411723]
 [-18.83038067  -4.12001627  -1.43396349]]
 ```

Initial positions (x, y, z):
```bash
[[-19.30458052  -5.09142425  -1.71743116]
 [-18.95293991  -4.954115    -2.10290605]
 [-18.71318257  -4.56528173  -2.06411723]
 [-18.83038067  -4.12001627  -1.43396349]]
```

Initial velocities (vx, vy, vz):

```bash
[[-0.11754325  0.70240116  0.88894977]
 [-0.70086511  0.29593982  1.36838713]
 [ 0.          0.          0.        ]
 [-0.81023741 -0.79165406  0.40258876]]
 ```

#### Output Data:

Predicted final positions (x, y, z) for planets. Output shape [1, 4, 3]:
```bash
[[[-19.276588   -4.984411   -1.6507224]
  [-19.011372   -4.895467   -1.9541278]
  [-18.735363   -4.571579   -2.0373058]
  [-18.910467   -4.2423964  -1.4385887]]]
```  
Comparison with Actual Final Positions:

Actual final positions (x, y, z):
```bash
[[-19.31085362  -5.01661123  -1.63205759]
 [-19.0080462   -4.90257404  -1.96607783]
 [-18.71359161  -4.56564631  -2.06387754]
 [-18.90975347  -4.20430149  -1.40140385]]
``` 
This script now prints both the input values (planet masses, initial positions, and velocities) as well as the predicted and actual final positions of the planets for comparison.