# Privacy Policy For Vusic Player
## Last Updated: 20 October 2026
Vusic Player is a fully open-source media player. OceanCyan Tech believes privacy is a fundamental right, especially in open-source software. This policy outlines exactly what data is collected, why it is collected, and how it is handled.

## 1. Core Principle: Zero Personal Data
### We do not collect, store, or sell any personal data. Specifically:

* No Media Tracking: All media metadata—such as filenames, track titles, artists, album artwork, directory paths, and durations—is processed entirely on your local device solely to display your library and play your media. This data is never collected, logged, or transmitted to our servers or any third party.

* No User Profiles: We do not use persistent device identifiers (such as hardware UUIDs, IMEI, or advertising IDs), user accounts, or fingerprinting.

* No Third-Party Brokers: Collected data is never shared with, sold to, or monetized by third parties.

## 2. Information We Collect
### To improve stability and understand which features are useful, OceanCyan Tech collects minimal, anonymous technical metrics only if you explicitly choose to enable telemetry.

A. Feature Usage (Aggregated Metrics)
* What is collected: General application version, operating system type, and high-level feature interaction counts (e.g., whether the reverb feature was opened or subtitle editor was used).

* Purpose: Helps developers understand which features need maintenance and active development.

* Format: Daily or per-session aggregates without fine-grained activity timestamps.

B. Error and Crash Diagnostics

* What is collected: Stack traces, internal application error codes, technical hardware profiles (e.g., GPU driver family, audio backend), and general container formats (e.g., MKV, MP4) involved in a crash.

* Sanitization: All file system paths are stripped of local usernames (e.g., /home/username/ is replaced with ~/), and network stream URIs are stripped of query parameters, tokens, and credentials before transmission.

* Purpose: Diagnosing bugs and preventing application crashes across different hardware environments.

## 3. IP Addresses and Network Handling

### When metrics or crash reports are transmitted to our ingestion server:

* Your IP address is processed temporarily in memory solely to complete the network connection.

* IP addresses are either truncated (anonymized) or discarded entirely at the server edge and are never stored alongside usage or crash logs.

## 4. User Control and Transparency

* Opt-In / Opt-Out: Telemetry is completely optional. You can enable or disable it at any time directly in Settings > About & Help > Privacy and Usage.

* Open Source & Verifiable: The entire codebase—including network handling and telemetry routines—is open source. You can inspect the source code, review the exact payload schemas, or build the application from source with telemetry disabled via build flags.

## 5. Contact & Repository
* For questions, audits, or suggestions regarding our privacy practices, review our public issue tracker or contact the maintainers at:

* Repository: https://github.com/OceanCyanTech/Vusic-Player

* Contact Email: oceancyantech@gmail.com
