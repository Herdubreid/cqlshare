<script>
    import { onMount } from 'svelte';
    import copy from 'copy-to-clipboard';
    import Prism from "./prism/prism-core";
    import './prism/prism-celinql';
    import { textMapStore } from './stores';
    import { debounce } from './helpers';

    export let caller;
    export let id;

    let editId = `${id}-edit`;
    let textareaEl;
    let codeEl;
    let mounted = false;
    
    let text =  $textMapStore.get(editId);

    $: code = Prism.highlight(text, Prism.languages.celinql, "CelinQL")
        || "<em style='color: lightgray;'>Enter Query</em>";
    
    $: textHeight = mounted && text.length > 0
        ? textareaEl.scrollHeight > textareaEl.clientHeight
            ? `${textareaEl.scrollHeight}px`
            : (codeEl.clientHeight + 24) < textareaEl.clientHeight
                ? `${codeEl.clientHeight}px`
                : `${textareaEl.clientHeight}px`
        : '1px';

    const setText = (ev) => {
        let letter = 0;
        text = "";
        if (ev.detail.text.length > 0)
        {
            const typewriter = () => {
                text += ev.detail.text[letter];
                letter++;
                if (letter === ev.detail.text.length) {
                    clearInterval(timer);                
                    $textMapStore.set(editId, text);
                    caller.invokeMethodAsync("TextChanged", id, text);
                }
            }
            const timer = setInterval(typewriter, 100);
        }
    };

    const updateText = debounce(() => {
        $textMapStore.set(editId, text);
        caller.invokeMethodAsync("TextChanged", id, text);
    }, 1000);

    onMount(() => {
        mounted = true;
    });
</script>

<style>
    div.col {
        overflow-y: auto;
	}
    textarea,
    code {
        margin: 0px;
        padding: 0px;
        border: 0;
        left: 0;
        word-break: break-all;
        white-space: break-spaces;
        overflow: visible;
        position: absolute;
        font-family: inherit;
        font-size: 16px;
    }
    textarea {
        width: 100%;
        min-height: 100%;
        background: transparent !important;
        z-index: 2;
        resize: none;
        -webkit-text-fill-color: transparent;
    }
    textarea:focus {
        outline: 0;
        border: 0;
        box-shadow: none;
    }
    code {
        z-index: 1;
    }
    pre {
        margin: 0px;
        white-space: pre-wrap;
	    word-wrap: break-word;
        font-family: inherit;
    }
    button {
        position: absolute;
        right: 2px;
        z-index: 3;
    }
</style>

<div class="col">
    <div>
        <textarea id={`${id}-textarea`} bind:value={text} on:set-text={setText} on:keyup={updateText} spellcheck="false" style="height: {textHeight}" bind:this={textareaEl} class="editor" />
        <pre>
            <code bind:this={codeEl}>{@html code}</code>
        </pre>
        <button type="button"
                class="btn btn-outline-info"
                data-toggle="tooltip"
                data-placement="bottom"
                title="Copy to Clipboard"
                on:click={() => copy(text)}>
            <i class="far fa-copy"></i>
        </button>
    </div>
</div>
