//NEW REVISED: 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using FlyleafLib.MediaFramework.MediaFrame;
using FlyleafLib.MediaPlayer;
using Vortice.Multimedia;
using Vortice.XAudio2;

namespace Vusic_Player;

public class SingleDevicePipeline : IDisposable
{
    public string DeviceId { get; }
    private IXAudio2? _xaudio2;
    private IXAudio2MasteringVoice? _masteringVoice;
    private IXAudio2SourceVoice? _sourceVoice;
    private float _volume = 1.0f;
    private bool _isMuted = false;

    public SingleDevicePipeline(string deviceId)
    {
        DeviceId = deviceId;
    }
    public bool EnsureFormat(int sampleRate, int channels, int bitsPerSample)
    {
        if (_xaudio2 == null) return false;

        // Skip recreation if the frame format is identical
        if (_sourceVoice != null &&
            CurrentSampleRate == sampleRate &&
            CurrentChannels == channels)
        {
            return true;
        }

        if (_sourceVoice != null)
        {
            _sourceVoice.BufferEnd -= OnBufferEnd;
            _sourceVoice.Stop();
            _sourceVoice.DestroyVoice();
            _sourceVoice.Dispose();
            _sourceVoice = null;
        }

        CurrentSampleRate = sampleRate;
        CurrentChannels = channels;

        // Build wave format dynamically matching the incoming PCM frame
        var waveFormat = new WaveFormat(sampleRate, bitsPerSample, channels);

        _sourceVoice = _xaudio2.CreateSourceVoice(
            waveFormat,
            VoiceFlags.None,
            2.0f,
            false
        );

        _sourceVoice.BufferEnd += OnBufferEnd;
        _sourceVoice.Start();
        ApplyVolume();

        return true;
    }

    public void SetVolume(float volume)
    {
        if (float.IsNaN(volume) || float.IsInfinity(volume)) volume = 0.0f;
        _volume = Math.Clamp(volume, 0.0f, 1.0f);

        ApplyVolume();
    }

    public void SetMute(bool isMuted)
    {
        _isMuted = isMuted;
        ApplyVolume();
    }

    private void ApplyVolume()
    {
        float targetVolume = _isMuted ? 0.0f : _volume;

        // Drive volume on the source voice
        if (_masteringVoice != null)
        {
            _masteringVoice.SetVolume(targetVolume);
        }

        // Direct software voice gain control
        if (_sourceVoice != null)
        {
            _sourceVoice.SetVolume(targetVolume);
        }
    }

    public float GetVolume() => _volume;
    public bool IsMuted() => _isMuted;

    public bool Initialize()
    {
        try
        {
            _xaudio2 = XAudio2.XAudio2Create(ProcessorSpecifier.DefaultProcessor);

            _masteringVoice = _xaudio2.CreateMasteringVoice(
                inputChannels: 0,
                inputSampleRate: 0,
                 0,
                deviceId: DeviceId
            );

            // Default to standard 44.1kHz stereo until first frame arrives
            var waveFormat = new WaveFormat(48000, 16, 2);

            _sourceVoice = _xaudio2.CreateSourceVoice(
                waveFormat,
                VoiceFlags.None,
                2.0f,
                false
            );

            _sourceVoice.BufferEnd += OnBufferEnd;
            _sourceVoice.Start();

            ApplyVolume();
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[XAudio2] Failed to initialize device {DeviceId}: {ex.Message}");
            Dispose();
            return false;
        }
    }
    public int CurrentSampleRate { get; private set; }
    public int CurrentChannels { get; private set; }
    public bool ReconfigureSourceVoice(int sampleRate, int channels, int bitsPerSample = 16)
    {
        if (_xaudio2 == null) return false;

        // Skip recreation if format hasn't changed
        if (_sourceVoice != null && CurrentSampleRate == sampleRate && CurrentChannels == channels)
            return true;

        if (_sourceVoice != null)
        {
            _sourceVoice.BufferEnd -= OnBufferEnd;
            _sourceVoice.Stop();
            _sourceVoice.DestroyVoice();
            _sourceVoice.Dispose();
            _sourceVoice = null;
        }

        CurrentSampleRate = sampleRate;
        CurrentChannels = channels;

        var waveFormat = new WaveFormat(sampleRate, bitsPerSample, channels);

        _sourceVoice = _xaudio2.CreateSourceVoice(
            waveFormat,
            VoiceFlags.None,
            2.0f,
            false
        );

        _sourceVoice.BufferEnd += OnBufferEnd;
        _sourceVoice.Start();
        ApplyVolume();
        return true;
    }
    private void OnBufferEnd(IntPtr context)
    {
        // Free the unmanaged buffer passed in context
        if (context != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(context);
        }
    }
    public event EventHandler<string>? DeviceDisconnected;
    public void SubmitData(byte[] buffer)
    {
        if (_sourceVoice == null || buffer.Length == 0) return;
        try
        {
            IntPtr nativeBuffer = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, nativeBuffer, buffer.Length);

            var audioBuffer = new AudioBuffer
            {
                AudioDataPointer = nativeBuffer,
                AudioBytes = (uint)buffer.Length,
                Flags = BufferFlags.None,
                Context = nativeBuffer // Pass pointer as Context to free on BufferEnd
            };

            _sourceVoice.SubmitSourceBuffer(audioBuffer);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Device Lost: " + DeviceId + ". Expected Error: " + ex.Message) ;
            DeviceDisconnected?.Invoke(this, DeviceId);
        }
    }

    public void Dispose()
    {
        if (_sourceVoice != null)
        {
            _sourceVoice.BufferEnd -= OnBufferEnd;
            _sourceVoice.Stop();
            _sourceVoice.DestroyVoice();
            _sourceVoice.Dispose();
            _sourceVoice = null;
        }

        if (_masteringVoice != null)
        {
            _masteringVoice.DestroyVoice();
            _masteringVoice.Dispose();
            _masteringVoice = null;
        }

        _xaudio2?.Dispose();
        _xaudio2 = null;
    }
}
public class XAudio2MultiOutputEngine : IDisposable
{
    private readonly Player _player;
    private readonly List<SingleDevicePipeline> _pipelines = new();
    private bool _isInitialized;
    private readonly object _lock = new();

    public XAudio2MultiOutputEngine(Player player)
    {
        _player = player ?? throw new ArgumentNullException(nameof(player));

        // Mute Flyleaf's built-in single audio target
        _player.Audio.Mute = true;
        _player.Audio.SamplesAdded += OnSamplesAdded;
    }
    public void SetDeviceVolume(string deviceID, float volume)
    {
        lock (_lock)
        {
            System.Diagnostics.Debug.WriteLine($"[SetDeviceVolume] Target ID: '{deviceID}' | Target Volume: {volume}");

            var pipeline = _pipelines.Find(p => p.DeviceId == deviceID);

            if (pipeline != null)
            {
                pipeline.SetVolume(volume);
                System.Diagnostics.Debug.WriteLine($"[SetDeviceVolume] SUCCESS: Found pipeline for '{deviceID}' and updated volume.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[SetDeviceVolume] ERROR: No matching pipeline found for ID '{deviceID}'!");
                System.Diagnostics.Debug.WriteLine($"[SetDeviceVolume] Currently registered pipelines count: {_pipelines.Count}");

                foreach (var p in _pipelines)
                {
                    System.Diagnostics.Debug.WriteLine($"[SetDeviceVolume] Registered Pipeline ID: '{p.DeviceId}'");
                }
            }
        }
    }
    public void MuteDevice(string deviceID)
    {
        lock (_lock)
        {
            var pipeline = _pipelines.Find(p => p.DeviceId == deviceID);
            pipeline?.SetMute(true);
        }
    }
    public void UnmuteDevice(string deviceID)
    {
        lock (_lock)
        {
            var pipeline = _pipelines.Find(p => p.DeviceId == deviceID);
            pipeline?.SetMute(false);
        }
    }
    public float GetDeviceVolume(string deviceId)
    {
        lock (_lock)
        {
            var pipeline = _pipelines.Find(p => p.DeviceId == deviceId);
            return pipeline?.GetVolume() ?? 0.0f;
        }
    }
    public void InitializeOutputs(List<string> deviceIds)
    {
        lock (_lock)
        {
            StopAndClear();

            foreach (var deviceId in deviceIds)
            {
                var pipeline = new SingleDevicePipeline(deviceId);
                if (pipeline.Initialize())
                {
                    _pipelines.Add(pipeline);
                }
                pipeline.DeviceDisconnected += Pipeline_DeviceDisconnected;
            }

            if (_pipelines.Count > 0)
            {
                _isInitialized = true;
            }
        }
    }
    public event EventHandler<string>? DeviceDisconnected;
    private void Pipeline_DeviceDisconnected(object? sender, string deviceID)
    {
        lock (_lock)
        {
            var pipeline = _pipelines.FirstOrDefault(p => p.DeviceId == deviceID);
            if (pipeline != null)
            {
                pipeline.DeviceDisconnected -= Pipeline_DeviceDisconnected;
                pipeline.Dispose();
                _pipelines.Remove(pipeline);

                System.Diagnostics.Debug.WriteLine($"[XAudio2Engine] Removed device: {deviceID}");
            }
        }

        // 5. This in turn calls PlayerService's event handler!
        DeviceDisconnected?.Invoke(this, deviceID);
    }

    private void OnSamplesAdded(object? sender, AudioFrame aFrame)
    {
        if (!_isInitialized || aFrame.dataPtr == IntPtr.Zero || aFrame.dataLen <= 0)
            return;

        // Extract exact frame parameters emitted by Flyleaf
        var audioStream = _player.Audio;
        int sampleRate = audioStream != null && audioStream.SampleRate > 0 ? audioStream.SampleRate : 48000;
        int channels = audioStream != null && audioStream.Channels > 0 ? audioStream.Channels : 2;
        int bitsPerSample = audioStream != null && audioStream.Bits > 0 ? audioStream.Bits : 16;

        lock (_lock)
        {
            byte[] pcmData = new byte[aFrame.dataLen];
            Marshal.Copy(aFrame.dataPtr, pcmData, 0, aFrame.dataLen);

            foreach (var pipeline in _pipelines)
            {
                pipeline.EnsureFormat(sampleRate, channels, bitsPerSample);
                pipeline.SubmitData(pcmData);
            }
        }
    }
    public void StopAndClear()
    {
        lock (_lock)
        {
            _isInitialized = false;

            foreach (var pipeline in _pipelines)
            {
                pipeline.DeviceDisconnected -= Pipeline_DeviceDisconnected;
                pipeline.Dispose();
            }
            _pipelines.Clear();
        }
    }

    public void Dispose()
    {
        _player.Audio.SamplesAdded -= OnSamplesAdded;
        StopAndClear();
        GC.SuppressFinalize(this);
    }
}