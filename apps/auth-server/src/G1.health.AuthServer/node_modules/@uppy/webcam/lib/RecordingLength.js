import { h } from 'preact';
import formatSeconds from "./formatSeconds.js";
export default function RecordingLength(_ref) {
  let {
    recordingLengthSeconds,
    i18n
  } = _ref;
  const formattedRecordingLengthSeconds = formatSeconds(recordingLengthSeconds);
  return h("span", {
    "aria-label": i18n('recordingLength', {
      recording_length: formattedRecordingLengthSeconds
    })
  }, formattedRecordingLengthSeconds);
}