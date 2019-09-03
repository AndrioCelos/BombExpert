These documents describe the protocol the bot currently recognises. The bot will do her best to match your voice input to one of the commands described in these documents.

Wait for the bot to respond before saying your next command.

The following commands may be used at any time:

  - `new bomb`: Resets all edgework and strikes variables, allowing you to start working on a new bomb.
  - `defuse [module]`: Tells the bot that you intend to begin work on the specified module. This enables the commands specific to that module.
  - `strike [number]`: Sets the number of strikes. This is important for Simon Says.
  - `verify code`: Asks for the verification code from the manual.
  - `verify [batteries|indicators|ports|serial]`: Asks for all known information on the specified edgework.
  - `verify strikes`: Asks for the current number of strikes.
  - `verify rule seed`: Asks for the current rule seed.
  - `edgework`: Starts a routine to provide all edgework.
  - `batteries`: Allows you to tell the bot the number of batteries and holders.
  - `indicators`: Allows you to tell the bot the indicators.
  - `ports`: Allows you to tell the bot the port plates.
  - `serial number`: Allows you to tell the bot the serial number.
  - `solver test`: Runs a simple test to check that the C# library is loaded correctly. If the test is unsuccessful, a pure AIML solution without rule seed mod support will be used as a fallback. Some mod modules may not be supported in this mode, though all vanilla modules are.
  - `rule seed [number]`: Specifies the rule seed. The vanilla game uses `1`. This is not reset between bombs.

Rule seed support is available for any rule seed with the C# library. If the library is unavailable, an AIML manual must be provided for a given rule seed (other than 1) before the bot can use it. The Solver Tester application can be used to generate these files.

## Edgework

When asked to provide edgework, we recognise the following responses:

### Batteries

  - `[n] batteries in [m] holders`
  - `[0|1] battery`

### Indicators

List the indicators by their state and label. Example: `lit CAR, unlit freak, unlit november S A`. Say `done` after listing them. You can also say `no indicators`.

### Ports

List the port plates. For each plate, say `empty plate` if it is empty; otherwise `plate` followed by a list of ports on the plate. Example: `plate serial, parallel; empty plate; plate RCA, RJ, DVI`. Say `done` after listing them. You can also say `no ports`.

### Serial number

Read the serial number using the NATO phonetic alphabet. Example: `alfa bravo 3 delta echo 6`.
