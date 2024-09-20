function _classPrivateFieldLooseBase(receiver, privateKey) { if (!Object.prototype.hasOwnProperty.call(receiver, privateKey)) { throw new TypeError("attempted to use private field on non-instance"); } return receiver; }
var id = 0;
function _classPrivateFieldLooseKey(name) { return "__private_" + id++ + "_" + name; }
const isSupported = typeof navigator !== 'undefined' && 'serviceWorker' in navigator;
function waitForServiceWorker() {
  return new Promise((resolve, reject) => {
    if (!isSupported) {
      reject(new Error('Unsupported'));
    } else if (navigator.serviceWorker.controller) {
      // A serviceWorker is already registered and active.
      resolve();
    } else {
      navigator.serviceWorker.addEventListener('controllerchange', () => {
        resolve();
      });
    }
  });
}
var _ready = /*#__PURE__*/_classPrivateFieldLooseKey("ready");
class ServiceWorkerStore {
  constructor(opts) {
    Object.defineProperty(this, _ready, {
      writable: true,
      value: void 0
    });
    _classPrivateFieldLooseBase(this, _ready)[_ready] = waitForServiceWorker().then(val => {
      _classPrivateFieldLooseBase(this, _ready)[_ready] = val;
    });
    this.name = opts.storeName;
  }
  get ready() {
    return Promise.resolve(_classPrivateFieldLooseBase(this, _ready)[_ready]);
  }

  // TODO: remove this setter in the next major
  set ready(val) {
    _classPrivateFieldLooseBase(this, _ready)[_ready] = val;
  }
  async list() {
    await _classPrivateFieldLooseBase(this, _ready)[_ready];
    return new Promise((resolve, reject) => {
      const onMessage = event => {
        if (event.data.store !== this.name) {
          return;
        }
        switch (event.data.type) {
          case 'uppy/ALL_FILES':
            resolve(event.data.files);
            navigator.serviceWorker.removeEventListener('message', onMessage);
            break;
          default:
            reject();
        }
      };
      navigator.serviceWorker.addEventListener('message', onMessage);
      navigator.serviceWorker.controller.postMessage({
        type: 'uppy/GET_FILES',
        store: this.name
      });
    });
  }
  async put(file) {
    await _classPrivateFieldLooseBase(this, _ready)[_ready];
    navigator.serviceWorker.controller.postMessage({
      type: 'uppy/ADD_FILE',
      store: this.name,
      file
    });
  }
  async delete(fileID) {
    await _classPrivateFieldLooseBase(this, _ready)[_ready];
    navigator.serviceWorker.controller.postMessage({
      type: 'uppy/REMOVE_FILE',
      store: this.name,
      fileID
    });
  }
}
ServiceWorkerStore.isSupported = isSupported;
export default ServiceWorkerStore;