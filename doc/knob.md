# On the Subject of Knobs

Say `defuse knob` to select a Knob module.

The bot will tell you which light states you must read. For the vanilla game, this is the six lights on the left side of the module. Each light is `on` or `off`. `all on` and `all off` are also recognised. The bot will tell you which direction to turn the knob to. This is with respect to the 'Up' label; for instance, if the bot says 'up', you must turn the knob to wherever the 'Up' label actually is.

## Example conversation

![Example knob](images/exampleknob.png)

>**Defuser**: Defuse knob\
>**Bot**: Read the lights on the left side of the module: 'on' or 'off' in reading order.\
>**Defuser**: Off off off on off off\
>**Bot**: Turn the knob to the left position, with respect to the 'Up' label.
