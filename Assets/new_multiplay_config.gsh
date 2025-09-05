version: 1.0
builds:
  my build: # replace with the name for your build
    executableName: Build.x86_64 # the name of your build executable
    buildPath: Builds/Multiplay # the location of the build files
    excludePaths: # paths to exclude from upload (supports only basic patterns)
      - Builds/Multiplay/DoNotShip/*.*
      - Builds/Multiplay/Server_Data/app.info
buildConfigurations:
  my build configuration: # replace with the name for your build configuration
    build: my build # replace with the name for your build
    queryType: sqp # sqp or a2s, delete if you do not have logs to query
    binaryPath: Build.x86_64 # the name of your build executable
    commandLine: -port $$port$$ -queryport $$query_port$$ -log $$log_dir$$/Engine.log # launch parameters for your server
    variables: {}
fleets:
  my fleet: # replace with the name for your fleet
    buildConfigurations:
      - my build configuration # replace with the names of your build configuration
    regions:
      North America: # North America, Europe, Asia, South America, Australia
        minAvailable: 0 # minimum number of servers running in the region
        maxServers: 1 # maximum number of servers running in the region
    usageSettings:
      - hardwareType: CLOUD #The hardware type of a machine. Can be CLOUD or METAL.
        machineType: GCP-N2 # Machine type to be associated with these setting.   * For CLOUD setting: In most cases, the only machine type available for your fleet is GCP-N2.   * For METAL setting: Please omit this field. All metal machines will be using the same setting, regardless of its type.
        maxServersPerMachine: 2 # Maximum number of servers to be allocated per machine.   * For CLOUD setting: This is a required field.   * For METAL setting: This is an optional field.
