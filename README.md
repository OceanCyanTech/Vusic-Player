# Vusic Player (Coming Soon)
<img width="1917" height="1078" alt="Screenshot 2026-11-09 155633" src="https://github.com/user-attachments/assets/6aff9822-58d9-4ecf-b143-85d141e39ec8" />
<img width="1916" height="1073" alt="Screenshot 2026-11-09 155703" src="https://github.com/user-attachments/assets/e696aeea-99ea-4d2a-bca7-f46ec03929e0" />
<img width="1915" height="1077" alt="Screenshot 2026-11-09 155759" src="https://github.com/user-attachments/assets/2ea9a44a-bc92-49f1-ad06-24fa8ed9772b" />
<img width="1917" height="1078" alt="Screenshot 2026-11-09 155745" src="https://github.com/user-attachments/assets/377f3bd7-71ca-4079-91ec-10134fc99efa" />
<img width="1917" height="1078" alt="Screenshot 2026-11-09 155822" src="https://github.com/user-attachments/assets/229f885a-2506-424a-a629-d73f55c6f828" />
<img width="1917" height="1076" alt="Screenshot 2026-11-09 155714" src="https://github.com/user-attachments/assets/7d438eba-09ba-4de0-989f-a1bff4b8b450" />


## ⚡ Core Features

### 🎬 Universal Playback Engine
* **No External Codecs Required:** Play MP4, MKV, AVI, MOV, FLV, MP3, FLAC, WAV, OGG, and dozens of other container formats and codecs out of the box.
* **DirectX Hardware Acceleration:** Smooth, low-latency video decoding and presentation powered by Flyleaf and DirectX.
* **Real-Time Video Adjustments:** Fine-tune brightness, contrast, hue, saturation, and orientation on the fly during playback.
* **Smart Playback Memory:** Automatically remembers playback progress for videos and tracks recent media history.

### 🎧 Advanced Audio & DSP Routing
* **Simultaneous Multi-Device Output:** Route audio to multiple playback devices (e.g., wired headphones, Bluetooth earbuds, external speakers) at the same time with independent per-device volume controls.
* **Built-in Digital Reverb:** Apply studio reverb effects directly to audio streams with zero external DSP dependencies.

### ✍️ Subtitle & Lyric Suite
* **Online Lyric Fetcher:** Automatically search and retrieve synchronized lyrics from the web.
* **Built-in Lyric Editor & Live Preview:** Create, edit, and fine-tune timestamped lyrics with an interactive live display during playback.
* **Integrated Subtitle Editor:** Create, adjust timing, and edit subtitles directly inside the application.

### 🗂️ Media Management & Tagging
* **Unified Music & Video Library:** Automatically scans, indexes, and organizes your local media with rapid SQLite-backed search.
* **Batch Tag & Cover Art Editor:** Inspect and update metadata (Title, Artist, Album, Genre, Year) and embed cover art across multiple audio/video files simultaneously.
* **Online Artwork Search:** Search and retrieve high-resolution artist images and album covers directly within the app.
* **Playlists & Queues:** Drag-and-drop queue management, custom playlists, and full-folder playback.

### 🎨 Customization & Privacy
* **Handcrafted Visual Themes:** Choose between distinct color palettes and bespoke iconography tailored for modern Windows design.
* **100% Free & Open Source:** Licensed under GNU General Public License v3.0 (GPLv3).
* **Privacy-First:** Fully local processing with zero background telemetry, ads, or tracking.

---

## 🛠️ Built With

* **Framework:** [Windows App SDK](https://github.com/microsoft/WindowsAppSDK) (WinUI 3) & [.NET 8](https://dotnet.microsoft.com/)
* **Media Core:** [FlyleafLib](https://github.com/SuRGeoNix/Flyleaf) & [FFmpeg](https://ffmpeg.org/)
* **Audio Routing:** XAudio2 & Vortice.Windows
* **Metadata & Tagging:** [TagLib#](https://github.com/mono/taglib-sharp)
* **Local Storage:** SQLite
* **MVVM Architecture:** [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
