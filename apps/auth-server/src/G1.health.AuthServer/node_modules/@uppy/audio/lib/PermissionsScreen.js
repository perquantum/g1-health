import { h } from 'preact';
export default (props => {
  const {
    icon,
    hasAudio,
    i18n
  } = props;
  return h("div", {
    className: "uppy-Audio-permissons"
  }, h("div", {
    className: "uppy-Audio-permissonsIcon"
  }, icon()), h("h1", {
    className: "uppy-Audio-title"
  }, hasAudio ? i18n('allowAudioAccessTitle') : i18n('noAudioTitle')), h("p", null, hasAudio ? i18n('allowAudioAccessDescription') : i18n('noAudioDescription')));
});