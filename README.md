# GETTING STARTED
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
