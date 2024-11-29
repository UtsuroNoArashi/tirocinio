import toml
# from gatr.experiments.nbody import NBodyExperiment

def main():
    cfg = toml.load("GATr/config2/nbody.toml")

    name = cfg["run_name"]
    print(name)
    # exp = NBodyExperiment(cfg)
    # exp()

if __name__ == "__main__":
    main()