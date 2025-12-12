// Voice Chat JavaScript Module
// Handles audio capture and playback for Realtime API

window.voiceChat = {
    dotNetRef: null,
    audioContext: null,
    mediaStream: null,
    mediaRecorder: null,
    audioWorkletNode: null,
    isCapturing: false,
    
    // Audio playback queue
    audioQueue: [],
    isPlaying: false,
    currentPlaybackTime: 0,
    activeSources: [], // Track active audio sources for interruption
    debugVolume: false, // Set to true to see volume levels in console

    /**
     * Initialize the voice chat module
     */
    initialize: function (dotNetReference) {
        this.dotNetRef = dotNetReference;
        console.log('✅ Voice chat module initialized');
    },

    /**
     * Start capturing audio from microphone
     */
    startAudioCapture: async function () {
        try {
            console.log('🎤 Starting audio capture...');

            // Request microphone access
            this.mediaStream = await navigator.mediaDevices.getUserMedia({
                audio: {
                    channelCount: 1,
                    sampleRate: 24000,
                    echoCancellation: true,
                    noiseSuppression: true,
                    autoGainControl: true
                }
            });

            // Create AudioContext
            this.audioContext = new (window.AudioContext || window.webkitAudioContext)({
                sampleRate: 24000
            });

            const source = this.audioContext.createMediaStreamSource(this.mediaStream);

            // Use ScriptProcessorNode for compatibility (deprecated but widely supported)
            // Buffer size: 4096 samples = ~170ms at 24kHz (reasonable for real-time)
            const bufferSize = 4096;
            const processor = this.audioContext.createScriptProcessor(bufferSize, 1, 1);

            let lastSendTime = Date.now();
            const minSendInterval = 100; // Minimum 100ms between sends to avoid overload
            let lastInterruptTime = 0;
            const minInterruptInterval = 1000; // Minimum 1 second between interrupts

            processor.onaudioprocess = (e) => {
                if (!this.isCapturing) return;

                const now = Date.now();
                if (now - lastSendTime < minSendInterval) {
                    return; // Skip this chunk to reduce load
                }
                lastSendTime = now;

                const inputData = e.inputBuffer.getChannelData(0);
                
                // Check if user is speaking (volume threshold)
                // 閾値を高めに設定して環境ノイズを無視
                const volume = this.calculateVolume(inputData);
                const INTERRUPT_THRESHOLD = 0.05; // 通常の会話レベル
                
                // Debug: Show volume level (helpful for calibration)
                if (this.debugVolume && Math.random() < 0.05) { // Log 5% of chunks
                    console.log(`🎚️ Volume: ${volume.toFixed(4)} (threshold: ${INTERRUPT_THRESHOLD})`);
                }
                
                if (volume > INTERRUPT_THRESHOLD) { // User is clearly speaking
                    // Interrupt AI playback (local)
                    this.interruptPlayback();
                    
                    // Notify server to cancel response (throttled)
                    // Only send if we're actually playing audio
                    if (now - lastInterruptTime > minInterruptInterval && this.activeSources.length > 0) {
                        lastInterruptTime = now;
                        this.dotNetRef.invokeMethodAsync('InterruptResponse')
                            .catch(err => {
                                // Silently ignore "no active response" errors
                                if (!err.message || !err.message.includes('Cancellation failed')) {
                                    console.error('❌ Error interrupting response:', err);
                                }
                            });
                    }
                }
                
                // Convert Float32Array to PCM16
                const pcm16 = this.floatToPCM16(inputData);
                
                // Send to .NET
                this.sendAudioToServer(pcm16);
            };

            source.connect(processor);
            processor.connect(this.audioContext.destination);

            this.isCapturing = true;
            console.log('✅ Audio capture started');

        } catch (error) {
            console.error('❌ Error starting audio capture:', error);
            alert('マイクへのアクセスが拒否されました。ブラウザの設定を確認してください。');
        }
    },

    /**
     * Calculate audio volume (RMS)
     */
    calculateVolume: function (samples) {
        let sum = 0;
        for (let i = 0; i < samples.length; i++) {
            sum += samples[i] * samples[i];
        }
        return Math.sqrt(sum / samples.length);
    },

    /**
     * Interrupt currently playing audio
     */
    interruptPlayback: function () {
        // Stop all active audio sources
        this.activeSources.forEach(source => {
            try {
                source.stop();
            } catch (e) {
                // Source may have already stopped
            }
        });
        this.activeSources = [];
        
        // Reset playback time to current time
        if (this.audioContext) {
            this.currentPlaybackTime = this.audioContext.currentTime;
        }
        
        console.log('🛑 Interrupted AI playback');
    },

    /**
     * Stop capturing audio
     */
    stopAudioCapture: function () {
        console.log('🛑 Stopping audio capture...');
        
        this.isCapturing = false;

        if (this.mediaStream) {
            this.mediaStream.getTracks().forEach(track => track.stop());
            this.mediaStream = null;
        }

        if (this.audioContext) {
            // Reset playback time
            this.currentPlaybackTime = 0;
            
            // Don't close the context immediately, keep it for playback
            // this.audioContext.close();
            // this.audioContext = null;
        }

        console.log('✅ Audio capture stopped');
    },

    /**
     * Convert Float32Array to PCM16 (Int16)
     */
    floatToPCM16: function (float32Array) {
        const pcm16 = new Int16Array(float32Array.length);
        for (let i = 0; i < float32Array.length; i++) {
            // Clamp values to [-1, 1] and convert to 16-bit integer
            const s = Math.max(-1, Math.min(1, float32Array[i]));
            pcm16[i] = s < 0 ? s * 0x8000 : s * 0x7FFF;
        }
        return pcm16;
    },

    /**
     * Send audio data to .NET backend
     */
    sendAudioToServer: function (pcm16Data) {
        try {
            // Convert Int16Array to byte array (Uint8Array)
            const byteArray = new Uint8Array(pcm16Data.buffer);
            
            // Call .NET method - pass Uint8Array directly (Blazor will handle conversion)
            this.dotNetRef.invokeMethodAsync('SendAudioData', Array.from(byteArray))
                .catch(err => {
                    console.error('❌ Error sending audio to server:', err);
                    if (err.message) {
                        console.error('Error details:', err.message);
                    }
                });
        } catch (error) {
            console.error('❌ Error in sendAudioToServer:', error);
        }
    },

    /**
     * Play audio received from server
     */
    playAudio: async function (audioBytes) {
        try {
            if (!this.audioContext) {
                this.audioContext = new (window.AudioContext || window.webkitAudioContext)({
                    sampleRate: 24000
                });
                this.currentPlaybackTime = this.audioContext.currentTime;
            }
            
            // Resume AudioContext if suspended (browser policy)
            if (this.audioContext.state === 'suspended') {
                console.log('⚠️ AudioContext suspended, resuming...');
                await this.audioContext.resume();
            }

            // Convert byte array to Int16Array (PCM16)
            const int16Array = new Int16Array(audioBytes.length / 2);
            for (let i = 0; i < int16Array.length; i++) {
                int16Array[i] = (audioBytes[i * 2 + 1] << 8) | audioBytes[i * 2];
            }

            // Convert PCM16 to Float32Array
            const float32Array = new Float32Array(int16Array.length);
            for (let i = 0; i < int16Array.length; i++) {
                float32Array[i] = int16Array[i] / (int16Array[i] < 0 ? 0x8000 : 0x7FFF);
            }

            // Create AudioBuffer
            const audioBuffer = this.audioContext.createBuffer(
                1, // mono
                float32Array.length,
                24000 // sample rate
            );
            audioBuffer.getChannelData(0).set(float32Array);

            // Schedule playback sequentially
            const source = this.audioContext.createBufferSource();
            source.buffer = audioBuffer;
            source.connect(this.audioContext.destination);
            
            // Calculate when to start this chunk
            const now = this.audioContext.currentTime;
            const startTime = Math.max(now, this.currentPlaybackTime);
            
            source.start(startTime);
            
            // Track this source for interruption
            this.activeSources.push(source);
            
            // Remove from active sources when done
            source.onended = () => {
                const index = this.activeSources.indexOf(source);
                if (index > -1) {
                    this.activeSources.splice(index, 1);
                }
            };
            
            // Update the next playback time
            this.currentPlaybackTime = startTime + audioBuffer.duration;
            
            console.log(`🔊 Playing audio chunk (${audioBuffer.duration.toFixed(3)}s) at ${startTime.toFixed(3)}s, state: ${this.audioContext.state}`);

        } catch (error) {
            console.error('❌ Error playing audio:', error);
        }
    }
};
