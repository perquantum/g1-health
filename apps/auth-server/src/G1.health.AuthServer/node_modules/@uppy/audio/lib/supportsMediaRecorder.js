export default function supportsMediaRecorder() {
  var _MediaRecorder$protot;
  /* eslint-disable compat/compat */
  return typeof MediaRecorder === 'function' && typeof ((_MediaRecorder$protot = MediaRecorder.prototype) == null ? void 0 : _MediaRecorder$protot.start) === 'function';
  /* eslint-enable compat/compat */
}