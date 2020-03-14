import 'bootstrap';
import { get } from 'svelte/store';
import { textMapStore } from './stores';
import CqlEditor from './CqlEditor.svelte';
import CqlViewer from './CqlViewer.svelte';
import JsonViewer from './JsonViewer.svelte';

const cqlEditors = new Map();
const cqlEditorId = id => `${id}-edit`;
const cqlEditorTextareaId = id => `${id}-textarea`;
window.cqlEditor = {
    init: (caller, id, text) => {
        const target = document.getElementById(cqlEditorId(id));
        textMapStore.update(m => {
            m.set(cqlEditorId(id), text);
            return m;
        });
        const ed = new CqlEditor({
            target,
            props: {
                caller,
                id
            }
        });
        cqlEditors.set(id, ed);
    },
    clear: (id) => {
        get(textMapStore).delete(cqlEditorId(id));
        cqlEditors.get(id).$destroy();
    },
    getText: (id) => {
        var m = get(textMapStore);
        return m.get(cqlEditorId(id));
    },
    setText: (id, text) => {
        const el = document.getElementById(cqlEditorTextareaId(id));
        const ev = new CustomEvent('set-text', {
            bubbles: false,
            detail: {
                text
            }
        });
        el.dispatchEvent(ev);
    }
};

const cqlViewers = new Map();
const cqlViewerId = id => `${id}-view`;
window.cqlViewer = {
    init: (id, text) => {
        const target = document.getElementById(cqlViewerId(id));
        textMapStore.update(m => {
            m.set(cqlViewerId(id), text);
            return m;
        });
        cqlViewers.set(id, new CqlViewer({
            target,
            props: {
                id: cqlViewerId(id)
            }
        }));
    },
    clear: id => {
        get(textMapStore).delete(cqlViewerId(id));
        cqlViewers.get(id).$destroy();
    },
    setText: (id, text) => {
        textMapStore.update(m => {
            m.set(cqlViewerId(id), text);
            return m;
        });
    }
};

const jsonViewers = new Map();
const jsonViewerId = (id) => `${id}-view`;
window.jsonViewer = {
    init: (id, text) => {
        const target = document.getElementById(jsonViewerId(id));
        textMapStore.update(m => {
            m.set(jsonViewerId(id), text);
            return m;
        });
        jsonViewers.set(id, new JsonViewer({
            target,
            props: {
                id: jsonViewerId(id)
            }
        }));
    },
    clear: (id) => {
        get(textMapStore).delete(jsonViewer(id));
        jsonViewers.get(id).$destroy();
    },
    setText: (id, text) => {
        textMapStore.update(m => {
            m.set(jsonViewerId(id), text);
            return m;
        });
    }
};

window.saveAsFile = (filename, bytesBase64) => {
    const link = document.createElement('a');
    link.download = filename;
    link.href = `data:applicaton/octed-stream;base64,${bytesBase64}`;
    document.body.appendChild(link); // Needed for Firefox
    link.click();
    document.body.removeChild(link);
};
