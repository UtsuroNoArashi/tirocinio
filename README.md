**ARCHITECTURE**

[Link to architecture](https://app.diagrams.net/#G10ioNA3i7YScJ8ETAOgx_yFk_1T61TpLh#%7B%22pageId%22%3A%22t11QtQghpPKsjs9m8q-p%22%7D)

## 1. Getting started##

Clone the repository. 

```bash
git clone https://github.com/UtsuroNoArashi/tirocinio
```
## 2. Creating and running docker image##

Build the Docker image from inside the `GATr` folder. 
On Linux, this can be done using

```bash
cd GATr
docker build -f docker/Dockerfile --tag gatr:latest .
```

The commands for Windows are similar, but you need to start fist the Docker Engine

Once the image has built successfully, we can run a container that is based on it.

On Linux, the command that does this and also

- mounts the current working directory into the container
- changes to the current working directory
- exposes all of the host's GPUs in the container

```bash
docker run --rm -it -v $PWD:$PWD -w $PWD --gpus=all gatr:latest /bin/bash
```

for Windows 
```bash
docker run --rm -it -v "${PWD}:/workspace" -w /workspace --gpus=all gatr:latest /bin/bash
```

## 3. Running the experiment##
First, we need to generate training and evaluation datasets. From the `GATr` folder you need to execute : 

```bash
python scripts/generate_nbody_dataset.py
```