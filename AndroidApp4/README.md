## LED-dimming

The 5-bit DAC of the MCP2221A is unique and interesting, but it cannot drive an LED directly. I added an op-amp with a voltage follower configuration to act as a buffer. A *swiss Army knife* LM358 op-amp can be used, but a rail-to-rail one is better. I used NJM2732D, but I think Microchip MCP6232 will work just as well.

![wiring](https://github.com/ja1umi/usb_grove-android/blob/master/AndroidApp4/LED-dimming_anrdroid4_BB.png)

<video src="https://github.com/user-attachments/assets/7bf77a11-33ec-406f-9534-671f8fa2d98f" width="600" controls></video>
