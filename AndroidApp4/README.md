## LED-dimming

The 5-bit DAC of the MCP2221A is unique and interesting, but it cannot drive an LED directly. I added an op-amp with a voltage follower configuration to act as a buffer. A *swiss Army knife* LM358 op-amp can be used, but a rail-to-rail one is better. I used NJM2732D, but I think Microchip MCP6232 will work just as well.
