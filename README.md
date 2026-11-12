<div align="center">
  <img src="https://raw.githubusercontent.com/OceanCyanTech/Vusic-Player/refs/heads/master/Vusic%20Player/Assets/appicon.png" alt="Vusic Player Logo" width="96" height="96" />
  <h1>Vusic Player</h1>
  <p><strong>A modern, privacy-first media player &amp; audio DSP suite for Windows 10 &amp; 11.</strong></p>

  <p>
    <a href="https://apps.microsoft.com/detail/9MST111CFJPD">
      <img src="https://get.microsoft.com/images/en-us%20dark.svg" alt="Get it from Microsoft" width="180" />
    </a>
  </p>

  <p>
    <a href="https://github.com/OceanCyanTech/Vusic-Player/blob/master/LICENSE">
      <img src="https://img.shields.io/badge/License-GPLv3-38BDF8?style=for-the-badge" alt="License: GPLv3" />
    </a>
    <a href="https://dotnet.microsoft.com/">
      <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 8" />
    </a>
    <a href="https://github.com/microsoft/WindowsAppSDK">
      <img src="https://img.shields.io/badge/WinUI_3-Windows_App_SDK-0078D4?style=for-the-badge&logo=windows&logoColor=white" alt="WinUI 3" />
    </a>
  </p>

  <p>
    <a href="https://oceancyantech.github.io/Vusic-Player/">Website</a> &bull;
    <a href="https://oceancyantech.github.io/Vusic-Player/howtousevusicplayer.html">User Guide</a> &bull;
    <a href="https://oceancyantech.github.io/Vusic-Player/validtimestamps.html">Timestamp Formats</a> &bull;
    <a href="https://oceancyantech.github.io/Vusic-Player/about.html">About</a> &bull;
    <a href="https://oceancyantech.github.io/Vusic-Player/donate.html">Donate</a> &bull;
    <a href="https://oceancyantech.github.io/Vusic-Player/feedback.html">Send Feedback</a>
  </p>
</div>

---

## 📸 Screenshots

<img width="1917" height="1078" alt="Vusic Player Home Screen" src="https://github.com/user-attachments/assets/6aff9822-58d9-4ecf-b143-85d141e39ec8" />
<img width="1916" height="1073" alt="Vusic Player Library View" src="https://github.com/user-attachments/assets/e696aeea-99ea-4d2a-bca7-f46ec03929e0" />
<img width="1915" height="1077" alt="Vusic Player Playback & Controls" src="https://github.com/user-attachments/assets/2ea9a44a-bc92-49f1-ad06-24fa8ed9772b" />
<img width="1917" height="1078" alt="Vusic Player Audio DSP Tools" src="https://github.com/user-attachments/assets/377f3bd7-71ca-4079-91ec-10134fc99efa" />
<img width="1917" height="1078" alt="Vusic Player Tag & Artwork Editor" src="https://github.com/user-attachments/assets/229f885a-2506-424a-a629-d73f55c6f828" />
<img width="1917" height="1076" alt="Vusic Player Custom Themes & Settings" src="https://github.com/user-attachments/assets/7d438eba-09ba-4de0-989f-a1bff4b8b450" />

---

## ⚡ Core Features

### 🎬 Universal Playback Engine
- **No External Codecs Required:** Play MP4, MKV, AVI, MOV, FLV, MP3, FLAC, WAV, OGG, and dozens of other container formats and codecs out of the box.
- **DirectX Hardware Acceleration:** Smooth, low-latency video decoding and presentation powered by Flyleaf and DirectX.
- **Real-Time Video Adjustments:** Fine-tune brightness, contrast, hue, saturation, and orientation on the fly during playback.
- **Smart Playback Memory:** Automatically remembers playback progress for videos and tracks recent media history.

### 🎧 Advanced Audio & DSP Routing
- **Simultaneous Multi-Device Output:** Route audio to multiple playback devices (e.g., wired headphones, Bluetooth earbuds, external speakers) at the same time with independent per-device volume controls.
- **Live & Batch Digital Reverb:** Apply studio reverb presets (`Cathedral`, `Hall`, and custom room parameters) directly in memory during playback or batch-export entire folders in parallel.
- **Real-Time & Batch Pitch Control:** Adjust playback pitch smoothly on the fly for both audio and video, or batch-shift audio tracks by ratio, semitones, or octaves.

### ✍️ Subtitle & Lyric Suite
- **Online Lyric Fetcher:** Automatically search and retrieve synchronized lyrics from the web via LRCLIB.
- **Built-in Lyric Editor & Live Preview:** Create, edit, and fine-tune timestamped lyrics with an interactive live display during playback.
- **Integrated Subtitle Editor:** Extract embedded video subtitle tracks or create, adjust timing, and edit subtitles directly inside the application.

### 🗂️ Media Management & Tagging
- **Unified Music & Video Library:** Automatically scans, indexes, and organizes your local media with rapid SQLite-backed search.
- **Batch Tag & Cover Art Editor:** Inspect and update metadata (Title, Artist, Album, Genre, Year) and embed cover art across multiple audio/video files simultaneously.
- **Online Artwork Search:** Search and retrieve high-resolution artist images and album covers directly within the app.
- **Playlists & Queues:** Drag-and-drop queue management, custom playlists, and full-folder playback.

### 🎨 Customization & Privacy
- **Handcrafted Visual Themes:** Choose between distinct color palettes and bespoke iconography tailored for modern Windows design.
- **100% Free & Open Source:** Licensed under GNU General Public License v3.0 (GPLv3).
- **Privacy-First:** Fully local processing with zero background telemetry, ads, or tracking.

---

## 🛠️ Built With

- **Framework:** [Windows App SDK](https://github.com/microsoft/WindowsAppSDK) (WinUI 3) & [.NET 8](https://dotnet.microsoft.com/)
- **Media Core:** [FlyleafLib](https://github.com/SuRGeoNix/Flyleaf) & [FFmpeg](https://ffmpeg.org/)
- **Audio Routing & DSP:** XAudio2, [Vortice.Windows](https://github.com/amerkoleci/Vortice.Windows), & SoX Reverb
- **Metadata & Tagging:** [TagLib#](https://github.com/mono/taglib-sharp)
- **Local Storage:** SQLite
- **MVVM Architecture:** [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)

---

## 💖 Support & Feedback

- **Download:** Get the latest release on the [Microsoft Store](https://apps.microsoft.com/detail/9MST111CFJPD)
- **Issues & Suggestions:** Open a GitHub Issue or submit directly via the [Feedback Page](https://oceancyantech.github.io/Vusic-Player/feedback.html)
- **Support Development:** Vusic Player has zero paywalls. If you find it useful, you can support ongoing development via UPI or Ko-fi on the [Donate Page](https://oceancyantech.github.io/Vusic-Player/donate.html)

---

<div align="center">
  <a href="https://oceancyantech.github.io/Vusic-Player/about.html">
    <img src="https://raw.githubusercontent.com/OceanCyanTech/Vusic-Player/refs/heads/master/Vusic%20Player/Assets/OceanCyan%20Logo.png" alt="OceanCyan Tech Logo" height="36" />
  </a>
  <p>
    Developed by <a href="https://oceancyantech.github.io/Vusic-Player/about.html">OceanCyan Tech</a> &bull; Licensed under <a href="LICENSE">GPLv3</a>
  </p>
</div>
