/**
 * @mention system for chapter content textarea.
 * Usage: import { init, initPreviewClicks } from '/js/mention.js';
 */

let currentDropdown = null;
let currentTextarea = null;
let mentionStart = -1;
let currentQuery = '';
let cachedItems = []; // { id, displayName, description, type: 'npc'|'pc' }
let clickOutsideHandler = null;

function removeDropdown() {
    if (currentDropdown) {
        currentDropdown.remove();
        currentDropdown = null;
    }
    mentionStart = -1;
    currentQuery = '';
    if (clickOutsideHandler) {
        document.removeEventListener('mousedown', clickOutsideHandler);
        clickOutsideHandler = null;
    }
}

function getCaretCoords(textarea) {
    const div = document.createElement('div');
    const style = getComputedStyle(textarea);
    ['fontFamily', 'fontSize', 'fontWeight', 'wordWrap', 'whiteSpace',
     'borderLeftWidth', 'borderRightWidth', 'paddingLeft', 'paddingRight',
     'paddingTop', 'paddingBottom', 'lineHeight'].forEach(p => {
        div.style[p] = style[p];
    });
    div.style.position = 'absolute';
    div.style.visibility = 'hidden';
    div.style.top = '0';
    div.style.left = '0';
    div.style.width = textarea.offsetWidth + 'px';
    div.style.whiteSpace = 'pre-wrap';
    div.style.overflow = 'hidden';

    const text = textarea.value.substring(0, mentionStart);
    div.textContent = text;

    const span = document.createElement('span');
    span.textContent = textarea.value.substring(mentionStart) || '.';
    div.appendChild(span);
    document.body.appendChild(div);

    const spanRect = span.getBoundingClientRect();
    const divRect = div.getBoundingClientRect();
    document.body.removeChild(div);

    const taRect = textarea.getBoundingClientRect();
    return {
        top: taRect.top + (spanRect.top - divRect.top) + window.scrollY,
        left: taRect.left + (spanRect.left - divRect.left) + window.scrollX
    };
}

function showDropdown(filtered, textarea) {
    removeDropdown();
    if (filtered.length === 0) return;

    const coords = getCaretCoords(textarea);
    const lineHeight = parseInt(getComputedStyle(textarea).lineHeight) || 20;

    const dd = document.createElement('div');
    dd.className = 'mention-dropdown';
    dd.style.top = (coords.top + lineHeight + 4) + 'px';
    dd.style.left = Math.min(coords.left, window.innerWidth - 220) + 'px';

    filtered.slice(0, 8).forEach(item => {
        const el = document.createElement('div');
        el.className = 'mention-item';
        el.dataset.id = item.id;
        el.dataset.name = item.displayName;
        el.dataset.type = item.type;
        el.dataset.desc = item.description || '';
        const badge = item.type === 'pc'
            ? `<span class="mention-type-badge mention-type-pc">PJ</span>`
            : `<span class="mention-type-badge mention-type-npc">PNJ</span>`;
        el.innerHTML = `<span class="mention-item-name">${badge}${escapeHtml(item.displayName)}</span>`;
        if (item.description) {
            el.innerHTML += `<span class="mention-item-desc">${escapeHtml(item.description.slice(0, 60))}${item.description.length > 60 ? '…' : ''}</span>`;
        }
        el.addEventListener('mousedown', e => {
            e.preventDefault();
            insertMention(item.id, item.displayName, item.type, textarea);
        });
        dd.appendChild(el);
    });

    document.body.appendChild(dd);
    currentDropdown = dd;

    clickOutsideHandler = (e) => {
        if (!dd.contains(e.target) && e.target !== textarea) removeDropdown();
    };
    document.addEventListener('mousedown', clickOutsideHandler);
}

function insertMention(id, name, type, textarea) {
    const before = textarea.value.substring(0, mentionStart);
    const after = textarea.value.substring(textarea.selectionStart);
    const token = `@[${name}](${type}:${id})`;
    textarea.value = before + token + after;
    const newPos = mentionStart + token.length;
    textarea.setSelectionRange(newPos, newPos);
    textarea.dispatchEvent(new Event('input', { bubbles: true }));
    removeDropdown();
    textarea.focus();
}

function escapeHtml(str) {
    return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

function navigateDropdown(direction) {
    if (!currentDropdown) return false;
    const items = currentDropdown.querySelectorAll('.mention-item');
    const active = currentDropdown.querySelector('.mention-item.active');
    if (direction === 'down') {
        const next = active ? active.nextElementSibling : items[0];
        if (active) active.classList.remove('active');
        if (next) next.classList.add('active');
    } else {
        const prev = active ? active.previousElementSibling : null;
        if (active) active.classList.remove('active');
        if (prev) prev.classList.add('active');
    }
    return true;
}

export function init(textareaId, dotnetRef) {
    const textarea = document.getElementById(textareaId);
    if (!textarea) return;

    // Remove previous listeners if reinitializing
    const oldClone = textarea._mentionClone;
    if (oldClone) {
        textarea.removeEventListener('input', textarea._mentionInputHandler);
        textarea.removeEventListener('keydown', textarea._mentionKeyHandler);
        textarea.removeEventListener('blur', textarea._mentionBlurHandler);
    }

    cachedItems = [];
    currentTextarea = textarea;

    const inputHandler = async (e) => {
        const pos = textarea.selectionStart;
        const textBefore = textarea.value.substring(0, pos);
        const match = textBefore.match(/@([^@\s\[\]()\n]*)$/);

        if (match) {
            mentionStart = pos - match[0].length;
            currentQuery = match[1];

            if (cachedItems.length === 0) {
                cachedItems = await dotnetRef.invokeMethodAsync('GetMentionables');
            }

            const filtered = cachedItems.filter(n =>
                currentQuery === '' ||
                n.displayName.toLowerCase().includes(currentQuery.toLowerCase())
            );
            showDropdown(filtered, textarea);
        } else {
            removeDropdown();
        }
    };

    const keyHandler = (e) => {
        if (!currentDropdown) return;
        if (e.key === 'Escape') { e.preventDefault(); removeDropdown(); return; }
        if (e.key === 'ArrowDown') { e.preventDefault(); navigateDropdown('down'); return; }
        if (e.key === 'ArrowUp') { e.preventDefault(); navigateDropdown('up'); return; }
        if (e.key === 'Enter') {
            const active = currentDropdown.querySelector('.mention-item.active') ||
                           currentDropdown.querySelector('.mention-item');
            if (active) {
                e.preventDefault();
                insertMention(parseInt(active.dataset.id), active.dataset.name, active.dataset.type, textarea);
            }
        }
    };

    const blurHandler = () => {
        setTimeout(() => removeDropdown(), 150);
    };

    textarea.addEventListener('input', inputHandler);
    textarea.addEventListener('keydown', keyHandler);
    textarea.addEventListener('blur', blurHandler);
    textarea._mentionInputHandler = inputHandler;
    textarea._mentionKeyHandler = keyHandler;
    textarea._mentionBlurHandler = blurHandler;
}

export function initPreviewClicks(containerId, dotnetRef) {
    const container = document.getElementById(containerId);
    if (!container) return;
    container.querySelectorAll('[data-mention-type][data-mention-id]').forEach(span => {
        const clone = span.cloneNode(true);
        span.parentNode.replaceChild(clone, span);
        clone.addEventListener('click', () => {
            const type = clone.dataset.mentionType;
            const id = parseInt(clone.dataset.mentionId);
            if (type && !isNaN(id)) dotnetRef.invokeMethodAsync('OpenMentionDetail', type, id);
        });
    });
}

export function clearCache() {
    cachedItems = [];
}
