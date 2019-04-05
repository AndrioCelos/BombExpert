These documents describe the protocol the bot currently recognises. The bot will do her best to match your voice input to one of the commands described in these documents.

Wait for the bot to respond before saying your next command.

The following commands may be used at any time:

  - `new bomb`: Resets all edgework and strikes variables, allowing you to start working on a new bomb.
  - `defuse [module]`: Tells the bot that you intend to begin work on the specified module. This enables the commands specific to that module.
  - `strike [number]`: Sets the number of strikes. This is important for Simon Says.
  - `verify code`: Asks for the verification code from the manual.
  - `verify serial`: Asks for all known information on the bomb's serial number.
  - `verify batteries`: Asks for the total number of batteries on the bomb.
  - `verify strikes`: Asks for the current number of strikes.
  - `verify rule seed`: Asks for the current rule seed.
  - `bomb serial [odd|even|vowel|no vowel]`: Provides information on the bomb's serial number.
  - `bomb parallel [yes|no]`: States whether or not there is a parallel port.
  - `bomb battery [number]`: States the number of batteries on the bomb.
  - `rule seed [number]`: Specifies the rule seed. The vanilla game uses `1`. This is not reset between bombs.
