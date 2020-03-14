<script>
    import copy from 'copy-to-clipboard';
    import Prism from "./prism/prism-core";
    import './prism/prism-celinql';
    import { textMapStore } from './stores';

    export let id;

    $: code = Prism.highlight($textMapStore.get(id), Prism.languages.celinql, "CelinQL")
        || "<em style='color: lightgray;'>No Content</em>";

</script>
<style>
    div.col {
        overflow-y: auto;
    }
    code {
        left: 0;
        position: absolute;
        font-family: inherit;
        font-size: 16px;
    }
    pre {
        margin: 0px;
        white-space: pre-wrap;
	    word-wrap: break-word;
        font-family: inherit;
    }
    button {
        position: absolute;
        right: 66px;
    }
</style>

<div class="col">
    <pre>
        <code>{@html code}</code>
    </pre>
</div>
<button type="button"
        class="btn btn-outline-info"
        data-toggle="tooltip"
        data-placement="bottom"
        title="Copy to Clipboard"
        on:click={() => copy($textMapStore.get(id))}>
    <i class="far fa-copy"></i>
</button>
