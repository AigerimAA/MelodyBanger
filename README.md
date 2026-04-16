# MelodyBanger - Music Store Showcase

A single-page web application that generates a random music catalog with album covers, playable music and likes.

## Stack

**Backend (C# / .NET 8)**
- ASP.NET Core Web API
- SixLabors.ImageSharp & ImageSharp.Drawing - generate album covers
- DryWetMidi - create music 

**Frontend (HTML/CSS/JS)**
- Tone.js & @tonejs/midi - play MIDI in browser
- Font Awesome - icons

## Features

- Multi-language support (EN / RU / KZ)
- Table view with pagination & expandable rows
- Gallery view with infinite scroll
- Seed-based deterministic generation
- Fractional likes (0-10 with probabilities)
- Random album covers with text overlay
- Playable MIDI music (varies by genre)

## Run locally

```bash
dotnet run
