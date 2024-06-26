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
#define LEDS 25
#define RING_LEDS 24
#define CORE_LED 0

#define CYAN_R 58
#define CYAN_G 209
#define CYAN_B 228

SoftwareSerial BTserial(8, 9); // RX | TX

Adafruit_NeoPixel strip = Adafruit_NeoPixel(LEDS, PIN, NEO_GRB + NEO_KHZ800);

uint32_t black = strip.Color(0, 0, 0);
uint32_t red = strip.Color(255, 0, 0);
uint32_t green = strip.Color(0, 255, 0);
uint32_t blue = strip.Color(0, 0, 255);
uint32_t cyan = strip.Color(CYAN_R, CYAN_B, CYAN_G);
uint32_t cyan_dim = strip.Color(CYAN_R / 8, CYAN_G / 8, CYAN_B / 8);
uint32_t cyan_dim2 = strip.Color(CYAN_R / 16, CYAN_G / 16, CYAN_B / 16);
uint32_t halfWhite = strip.Color(150, 255, 255);
uint32_t white = strip.Color(255, 255, 255);

char tmpbuff[120] = "";
char buffer[120] = "pulse";
uint8_t bufflen = 7;

const float AREF = 5.2;

const int dim = 8;
const int dim2 = 16;
ColorPulser pulser(Color(CYAN_R / dim, CYAN_G / dim, CYAN_B / dim), Color(255 / dim, 255 / dim, 255 / dim));
ColorPulser pulser2(Color(CYAN_R / dim, CYAN_G / dim, CYAN_B / dim), Color(255 / dim, 255 / dim, 255 / dim));

void core(uint32_t color)
{
	strip.setPixelColor(CORE_LED, color);
	strip.show();
}

void ring(uint32_t color, uint32_t wait = 0)
{
	for (int16_t i = CORE_LED + 1; i < RING_LEDS + CORE_LED + 1; i++)
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

int count = 0;

void pulse()
{
	auto &c = pulser.Value();
	core(strip.Color(c.R, c.G, c.B));
	ring(strip.Color(c.R, c.G, c.B));
	delay(15);
	if (!pulser.Animate())
	{
		count++;
	}

	if (count == 10)
		count = 0;
}

void pulse2()
{
	auto &c = pulser2.Value();
	core(strip.Color(c.R, c.G, c.B));
	ring(strip.Color(c.R, c.G, c.B));
	delay(15);
	if (!pulser2.Animate())
	{
		count++;
	}

	if (count == 10)
		count = 0;
}

void startUp()
{
	core(cyan_dim);

	ring(cyan_dim, 25);
	delay(1000);
	ring(black, 25);

	core(black);
}

void batch()
{
	uint16_t ledindex = 0;
	int index = 1;

	while (ledindex < LEDS)
	{
		Serial.print("LED ");
		Serial.print(ledindex);
		Serial.print(" set to ");
		Serial.print((uint8_t)buffer[index]);
		Serial.print(',');
		Serial.print((uint8_t)buffer[index + 1]);
		Serial.print(',');
		Serial.println((uint8_t)buffer[index + 2]);

		strip.setPixelColor(ledindex, strip.Color((uint8_t)buffer[index], (uint8_t)buffer[index + 1], (uint8_t)buffer[index + 2]));
		index += 3;
		ledindex++;
	}

	strip.show();

	strcpy(buffer, "nop");
}

void individual()
{
	Serial.print("LED ");
	Serial.print((uint16_t)buffer[1]);
	Serial.print(" set to ");
	Serial.print((uint8_t)buffer[2]);
	Serial.print(',');
	Serial.print((uint8_t)buffer[3]);
	Serial.print(',');
	Serial.println((uint8_t)buffer[4]);

	strip.setPixelColor((uint16_t)buffer[1], strip.Color((uint8_t)buffer[2], (uint8_t)buffer[3], (uint8_t)buffer[4]));
	strip.show();

	strcpy(buffer, "nop");
}

void setring()
{
	for (int i = CORE_LED + 1; i < LEDS; i++)
	{
		strip.setPixelColor(i, strip.Color(buffer[1], buffer[2], buffer[3]));
	}

	strip.show();
}

void battery()
{
	int bat[2];
	bat[0] = analogRead(A0);
	delay(1000);
	bat[1] = analogRead(A0);

	float battery = ((bat[0] + bat[1]) / 2) / 1023.0 * AREF;

	delay(1000);

	Serial.println(battery);

	BTserial.print('\4');
	BTserial.print(battery);

	strcpy(buffer, "pulse");
}

int index = 0;

void operationMode()
{
	if (strcmp(buffer, "startup") == 0)
		startUp();

	if (strcmp(buffer, "pulse") == 0)
		pulse();

	if (strcmp(buffer, "dim") == 0)
	{
		core(cyan_dim2);
		ring(cyan_dim2);
		strcpy(buffer, "nop");
	}

	if (strcmp(buffer, "black") == 0)
	{
		core(black);
		ring(black);
		strcpy(buffer, "nop");
	}

	if (strcmp(buffer, "green") == 0)
	{
		core(green);
		ring(green);
		strcpy(buffer, "nop");
	}

	if (strcmp(buffer, "blue") == 0)
	{
		core(blue);
		ring(blue);
		strcpy(buffer, "nop");
	}

	if (strcmp(buffer, "red") == 0)
	{
		core(red);
		ring(red);
		strcpy(buffer, "nop");
	}

	if (strcmp(buffer, "batt") == 0)
		battery();

	if (strcmp(buffer, "nop") == 0)
		return;

	if (buffer[0] == 'i')
		batch();

	if (buffer[0] == 'c')
		individual();

	if (buffer[0] == 'r')
		setring();
}

void setup()
{
	Serial.begin(9600);
	BTserial.begin(38400);

	pinMode(A0, INPUT);

	strip.begin();

	operationMode();

	strip.show();
}

void loop()
{
	if (BTserial.available() > 0)
	{
		tmpbuff[index] = BTserial.read();

		if (tmpbuff[index] == '\n')
		{

			tmpbuff[index] = 0;
			Serial.println(tmpbuff);
			index = 0;

			memcpy(buffer, tmpbuff, 120);
		}
		else
		{
			index++;
		}
	}
	else
	{
		operationMode();
	}
}