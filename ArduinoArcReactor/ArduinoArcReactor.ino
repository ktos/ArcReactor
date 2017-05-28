/*
* ArcReactor
*
* Copyright (C) Marcin Badurowicz <m at badurowicz dot net> 2017
*
*
* Permission is hereby granted, free of charge, to any person obtaining
* a copy of this software and associated documentation files
* (the "Software"), to deal in the Software without restriction,
* including without limitation the rights to use, copy, modify, merge,
* publish, distribute, sublicense, and/or sell copies of the Software,
* and to permit persons to whom the Software is furnished to do so,
* subject to the following conditions:
*
* The above copyright notice and this permission notice shall be
* included in all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
* OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
* NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
* BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
* ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
* CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

#include <SoftwareSerial.h>
#include <Adafruit_NeoPixel.h>
#include "ColorPulser.h"

#define PIN 6
#define LEDS 24
#define RING_LEDS 24
#define CORE_LED 1

#define CYAN_R 58
#define CYAN_G 209
#define CYAN_B 228

SoftwareSerial BTserial(2, 3); // RX | TX

Adafruit_NeoPixel strip = Adafruit_NeoPixel(LEDS, PIN, NEO_GRB + NEO_KHZ800);

uint32_t black = strip.Color(0, 0, 0);
uint32_t cyan = strip.Color(CYAN_R, CYAN_B, CYAN_G);
uint32_t cyan_dim = strip.Color(CYAN_R / 8, CYAN_G / 8, CYAN_B / 8);
uint32_t halfWhite = strip.Color(150, 255, 255);
uint32_t white = strip.Color(255, 255, 255);

char buffer[100] = "startup";
uint8_t bufflen = 7;

void core(uint32_t color)
{
	//strip.setPixelColor(CORE_LED, color);
	//strip.show();
}

void ring(uint32_t color, uint32_t wait = 0)
{
	for (int16_t i = 0; i < RING_LEDS; i++)
	{
		strip.setPixelColor(i, color);
		if (wait != 0)
		{
			strip.show();
			delay(wait);
		}
	}
	strip.show();
}

void corePulse()
{
	const uint32_t WAIT = 15;
	int16_t count = 0;

	ring(cyan);

	ColorPulser pulser(Color(CYAN_R, CYAN_G, CYAN_B), Color(255, 255, 255));
	while (count < 10)
	{
		auto& c = pulser.Value();
		core(strip.Color(c.R, c.G, c.B));
		delay(WAIT);
		if (!pulser.Animate())
		{
			count++;
		}
	}
}

void ringPulse(int dim = 8)
{
	const uint32_t WAIT = 15;
	int16_t count = 0;

	core(cyan_dim);

	ColorPulser pulser(Color(CYAN_R / dim, CYAN_G / dim, CYAN_B / dim), Color(255 / dim, 255 / dim, 255 / dim));
	while (count < 10)
	{
		auto& c = pulser.Value();
		ring(strip.Color(c.R, c.G, c.B));
		delay(WAIT);
		if (!pulser.Animate())
		{
			count++;
		}
	}
}

void startUp()
{
	core(cyan);

	ring(cyan_dim, 25);
	delay(1000);
	ring(black, 25);	
}

void everyindividual()
{
	int index = 1;
	while (index < bufflen) {
		strip.setPixelColor(index - 1, strip.Color(buffer[index], buffer[index + 1], buffer[index + 2]));
		index += 3;
	}

	strip.show();
}

void individual()
{
	strip.setPixelColor(buffer[1], strip.Color(buffer[2], buffer[3], buffer[4]));
	strip.show();
}

void setup()
{
	Serial.begin(9600);
	BTserial.begin(9600);

	strip.begin();
	strip.show();
}

void loop()
{
	int avail = BTserial.available();
	if (avail > 0)
	{
		int index = 0;
		while (index < avail) {
			buffer[index] = BTserial.read();
			index++;
		}
		buffer[index] = 0;

		bufflen = index - 1;

		Serial.println(buffer);
	}

	if (strcmp(buffer, "startup") == 0)
		startUp();

	if (strcmp(buffer, "pulse") == 0)
		ringPulse();

	if (strcmp(buffer, "black") == 0)
		ring(black);

	if (buffer[0] == 'i')
		everyindividual();

	if (buffer[0] == 'c')
		individual();

	delay(150);
}