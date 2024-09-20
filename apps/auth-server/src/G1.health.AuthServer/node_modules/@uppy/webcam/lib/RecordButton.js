import { h } from 'preact';
export default function RecordButton(_ref) {
  let {
    recording,
    onStartRecording,
    onStopRecording,
    i18n
  } = _ref;
  if (recording) {
    return h("button", {
      className: "uppy-u-reset uppy-c-btn uppy-Webcam-button",
      type: "button",
      title: i18n('stopRecording'),
      "aria-label": i18n('stopRecording'),
      onClick: onStopRecording,
      "data-uppy-super-focusable": true
    }, h("svg", {
      "aria-hidden": "true",
      focusable: "false",
      className: "uppy-c-icon",
      width: "100",
      height: "100",
      viewBox: "0 0 100 100"
    }, h("rect", {
      x: "15",
      y: "15",
      width: "70",
      height: "70"
    })));
  }
  return h("button", {
    className: "uppy-u-reset uppy-c-btn uppy-Webcam-button",
    type: "button",
    title: i18n('startRecording'),
    "aria-label": i18n('startRecording'),
    onClick: onStartRecording,
    "data-uppy-super-focusable": true
  }, h("svg", {
    "aria-hidden": "true",
    focusable: "false",
    className: "uppy-c-icon",
    width: "100",
    height: "100",
    viewBox: "0 0 100 100"
  }, h("circle", {
    cx: "50",
    cy: "50",
    r: "40"
  })));
}