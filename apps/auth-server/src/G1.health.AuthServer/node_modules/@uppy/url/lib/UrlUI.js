function _classPrivateFieldLooseBase(receiver, privateKey) { if (!Object.prototype.hasOwnProperty.call(receiver, privateKey)) { throw new TypeError("attempted to use private field on non-instance"); } return receiver; }
var id = 0;
function _classPrivateFieldLooseKey(name) { return "__private_" + id++ + "_" + name; }
import { h, Component } from 'preact';
import { nanoid } from 'nanoid/non-secure';
var _handleSubmit = /*#__PURE__*/_classPrivateFieldLooseKey("handleSubmit");
class UrlUI extends Component {
  constructor(props) {
    super(props);
    this.form = document.createElement('form');
    Object.defineProperty(this, _handleSubmit, {
      writable: true,
      value: ev => {
        ev.preventDefault();
        const {
          addFile
        } = this.props;
        const preparedValue = this.input.value.trim();
        addFile(preparedValue);
      }
    });
    this.form.id = nanoid();
  }
  componentDidMount() {
    this.input.value = '';
    this.form.addEventListener('submit', _classPrivateFieldLooseBase(this, _handleSubmit)[_handleSubmit]);
    document.body.appendChild(this.form);
  }
  componentWillUnmount() {
    this.form.removeEventListener('submit', _classPrivateFieldLooseBase(this, _handleSubmit)[_handleSubmit]);
    document.body.removeChild(this.form);
  }
  render() {
    const {
      i18n
    } = this.props;
    return h("div", {
      className: "uppy-Url"
    }, h("input", {
      className: "uppy-u-reset uppy-c-textInput uppy-Url-input",
      type: "text",
      "aria-label": i18n('enterUrlToImport'),
      placeholder: i18n('enterUrlToImport'),
      ref: input => {
        this.input = input;
      },
      "data-uppy-super-focusable": true,
      form: this.form.id
    }), h("button", {
      className: "uppy-u-reset uppy-c-btn uppy-c-btn-primary uppy-Url-importButton",
      type: "submit",
      form: this.form.id
    }, i18n('import')));
  }
}
export default UrlUI;