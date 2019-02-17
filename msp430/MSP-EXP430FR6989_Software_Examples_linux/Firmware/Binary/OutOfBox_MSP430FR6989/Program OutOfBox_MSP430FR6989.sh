#!/bin/bash
cd "$(dirname "$0")"
rootDir=$(pwd)
cd $rootDir/../MSP430Flasher
export LD_LIBRARY_PATH=$(pwd)
clear

echo MSP-EXP430FR6989-LaunchPad Firmware Programmer
device=MSP430FR6989
firmware=OutOfBox_MSP430FR6989.txt

echo Programming $firmware into $device ......
./MSP430Flasher -n $device -w $rootDir/$firmware -v -g -z [VCC]

read -p "Press [Enter] to continue..."