import { h } from 'preact';
export default (_ref => {
  let {
    currentDeviceId,
    audioSources,
    onChangeSource
  } = _ref;
  return h("div", {
    className: "uppy-Audio-videoSource"
  }, h("select", {
    className: "uppy-u-reset uppy-Audio-audioSource-select",
    onChange: event => {
      onChangeSource(event.target.value);
    }
  }, audioSources.map(audioSource => h("option", {
    key: audioSource.deviceId,
    value: audioSource.deviceId,
    selected: audioSource.deviceId === currentDeviceId
  }, audioSource.label))));
});