/* eslint-disable jsx-a11y/media-has-caption */
import { h } from 'preact';
import { useEffect, useRef } from 'preact/hooks';
import RecordButton from "./RecordButton.js";
import RecordingLength from "./RecordingLength.js";
import AudioSourceSelect from "./AudioSourceSelect.js";
import AudioOscilloscope from "./audio-oscilloscope/index.js";
import SubmitButton from "./SubmitButton.js";
import DiscardButton from "./DiscardButton.js";
export default function RecordingScreen(props) {
  const {
    stream,
    recordedAudio,
    onStop,
    recording,
    supportsRecording,
    audioSources,
    showAudioSourceDropdown,
    onSubmit,
    i18n,
    onStartRecording,
    onStopRecording,
    onDiscardRecordedAudio,
    recordingLengthSeconds
  } = props;
  const canvasEl = useRef(null);
  const oscilloscope = useRef();

  // componentDidMount / componentDidUnmount
  useEffect(() => {
    return () => {
      oscilloscope.current = null;
      onStop();
    };
  }, [onStop]);

  // componentDidUpdate
  useEffect(() => {
    if (!recordedAudio) {
      oscilloscope.current = new AudioOscilloscope(canvasEl.current, {
        canvas: {
          width: 600,
          height: 600
        },
        canvasContext: {
          lineWidth: 2,
          fillStyle: 'rgb(0,0,0)',
          strokeStyle: 'green'
        }
      });
      oscilloscope.current.draw();
      if (stream) {
        const audioContext = new AudioContext();
        const source = audioContext.createMediaStreamSource(stream);
        oscilloscope.current.addSource(source);
      }
    }
  }, [recordedAudio, stream]);
  const hasRecordedAudio = recordedAudio != null;
  const shouldShowRecordButton = !hasRecordedAudio && supportsRecording;
  const shouldShowAudioSourceDropdown = showAudioSourceDropdown && !hasRecordedAudio && audioSources && audioSources.length > 1;
  return h("div", {
    className: "uppy-Audio-container"
  }, h("div", {
    className: "uppy-Audio-audioContainer"
  }, hasRecordedAudio ? h("audio", {
    className: "uppy-Audio-player",
    controls: true,
    src: recordedAudio
  }) : h("canvas", {
    ref: canvasEl,
    className: "uppy-Audio-canvas"
  })), h("div", {
    className: "uppy-Audio-footer"
  }, h("div", {
    className: "uppy-Audio-audioSourceContainer"
  }, shouldShowAudioSourceDropdown ? AudioSourceSelect(props) : null), h("div", {
    className: "uppy-Audio-buttonContainer"
  }, shouldShowRecordButton && h(RecordButton, {
    recording: recording,
    onStartRecording: onStartRecording,
    onStopRecording: onStopRecording,
    i18n: i18n
  }), hasRecordedAudio && h(SubmitButton, {
    onSubmit: onSubmit,
    i18n: i18n
  }), hasRecordedAudio && h(DiscardButton, {
    onDiscard: onDiscardRecordedAudio,
    i18n: i18n
  })), h("div", {
    className: "uppy-Audio-recordingLength"
  }, !hasRecordedAudio && h(RecordingLength, {
    recordingLengthSeconds: recordingLengthSeconds,
    i18n: i18n
  }))));
}