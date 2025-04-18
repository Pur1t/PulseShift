# PulseShift

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://www.apache.org/licenses/LICENSE-2.0)

> A Unity rhythm game featuring AI-generated chart patterns and streamlined tempo estimation.

---

## 📖 Table of Contents

- [Introduction](#introduction)
- [Integration](#integration)
- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)
- [Additional Restrictions](#additional-restrictions)
- [Attribution](#attribution)

---

## 🔍 Introduction

**PulseShift** is a Unity-based rhythm game that challenges players to hit notes in sync with the music. Built around procedural chart generation and robust tempo estimation, PulseShift delivers dynamic and engaging gameplay.

## 🔗 Integration

- **Pattern Generation**: Uses the [AI Pattern Generator](https://github.com/Pur1t/AI-Pattern-Generator) tool for creating chart data files.
- **Tempo Estimation**: Implements the method from [Streamlined Tempo Estimation Based on Autocorrelation and Cross-correlation With Pulses](https://www.researchgate.net/publication/265130658_Streamlined_Tempo_Estimation_Based_on_Autocorrelation_and_Cross-correlation_With_Pulses) for accurate BPM detection.

## ✨ Features

- AI-driven chart generation
- Accurate tempo estimation via autocorrelation & cross-correlation pulses
- Drag-and-drop integration of pattern files into Unity
- Support for multiple song chart files
- Cross-platform compatibility (Windows, macOS, Linux)

## 🚀 Installation

1. **Create a new Unity project**
2. **Replace folders**: Copy the following three folders from this repo into your Unity project root:
   - `Assets/Scripts`
   - `Assets/Charts`
   - `Assets/Prefabs`
3. **Reimport assets** in Unity (Assets → Reimport All).
4. **Setup StreamingAssets**:
   - In your project’s `StreamingAssets` folder, create two subfolders:
     - `/Scripts` – place the AI Pattern Generator executable here (see [Installation](https://github.com/Pur1t/AI-Pattern-Generator#installation)).
     - `/Charts` – drop your generated chart text files (e.g. `.txt`) here.
5. **Build & Play**: Open the Unity Build Settings, select your target platform, and build your game. Launch to start playing!

## ▶️ Usage

- **Generate charts**: Run the AI Pattern Generator executable with your configuration to produce structured text chart files (e.g., using the custom `.txt` format).
- **Import into Unity**: Place the generated files in `StreamingAssets/Charts`, then start the game to load charts automatically.

## 📄 License

Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License. You may obtain a copy at:

> https://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND.

## 🚫 Additional Restrictions

1. **Attribution**: Any derivative works, redistributions, or modifications must provide clear attribution to the Computer Science Department, Faculty of Science, Kasetsart University.
2. **Commercial Use**: Commercial use of this software or any derivative works requires explicit written permission from the Computer Science Department, Faculty of Science, Kasetsart University.

## 🎓 Attribution

Developed by Purit, Faculty of Science, Kasetsart University.
