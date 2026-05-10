import { JSDOM } from 'jsdom';
import { readFile } from 'node:fs/promises';
import { resolve } from 'node:path';
import assert from 'node:assert/strict';

const root = resolve('.');
const html = await readFile(resolve(root, 'src/ABDS.Web/wwwroot/index.html'), 'utf8');
const script = await readFile(resolve(root, 'src/ABDS.Web/wwwroot/app.js'), 'utf8');

const dom = new JSDOM(html, {
  url: 'http://localhost:5076/',
  runScripts: 'outside-only',
  pretendToBeVisual: true,
});

const { window } = dom;
const fetchCalls = [];
let clipboardText = '';

window.matchMedia = () => ({
  matches: false,
  addEventListener() {},
  removeEventListener() {},
});
window.HTMLDialogElement.prototype.showModal = function showModal() { this.open = true; };
window.HTMLDialogElement.prototype.close = function close() { this.open = false; };
window.navigator.clipboard = {
  writeText: async (value) => { clipboardText = value; },
};
window.alert = () => {};
window.prompt = () => 'D:\\PickedFolder';
window.fetch = async (url, options = {}) => {
  fetchCalls.push({ url: String(url), method: options.method || 'GET', body: options.body || null });
  if (url === '/api/config') {
    if (options.method === 'PUT') {
      return jsonResponse(JSON.parse(options.body));
    }
    return jsonResponse({
      serviceTickMinutes: 5,
      syncPairs: [{ sourcePath: 'D:\\CADProjects', targetPaths: ['E:\\Sync1'], enabled: true }],
      backupSources: [{ name: 'CADProjects', sourcePath: 'D:\\CADProjects', backupRootPath: 'F:\\Backups', enabled: true }],
      schedule: {
        autoSyncEnabled: true,
        autoSyncInterval: '00:15:00',
        syncComparisonMode: 1,
        hashBelowSizeMbThreshold: 20,
        autoBackupEnabled: true,
        autoBackupIntervalFromLastSuccess: '12:00:00',
        backupScheduleMode: 'weekly',
        backupScheduleTime: '04:00',
        backupScheduleWeekDays: [1],
        backupScheduleMonthDays: [1],
        backupArchiveFormat: 0,
        backupCompressionPreset: 2,
        syncOnAppStart: true,
        syncOnAppExit: false,
        maxBackupStorageBytes: 322122547200,
      },
      update: {
        manifestUrl: 'https://abds.sokolowskifilip.pl/update/version',
      },
      criticalBackupOverdueExtra: '02:00:00',
    });
  }
  if (url === '/api/diagnostics/paths') {
    return jsonResponse({
      rootDir: 'C:\\ProgramData\\ABDS',
      configPath: 'C:\\ProgramData\\ABDS\\config.json',
      statePath: 'C:\\ProgramData\\ABDS\\state.json',
      hashCachePath: 'C:\\ProgramData\\ABDS\\hashcache.json',
      dumpsDir: 'C:\\ProgramData\\ABDS\\Dumps',
    });
  }
  if (url === '/api/windows/startup') {
    if (options.method === 'PUT') {
      return jsonResponse({ enabled: JSON.parse(options.body).enabled, runValueName: 'ABDS Tray Agent', trayAgentPath: 'C:\\Program Files\\ABDS\\ABDS.TrayAgent.exe', message: null });
    }
    return jsonResponse({ enabled: true, runValueName: 'ABDS Tray Agent', trayAgentPath: 'C:\\Program Files\\ABDS\\ABDS.TrayAgent.exe', message: null });
  }
  if (url === '/api/status') {
    return jsonResponse({
      traySeverity: 'Ok',
      trayTooltip: 'ABDS: OK',
      hasRunningJob: false,
      runningRunId: null,
      syncStatuses: [{ sourcePath: 'D:\\CADProjects', targetPath: 'E:\\Sync1', lastState: 'Success', lastSuccessAt: '2026-05-06T12:00:00+02:00', lastAttemptAt: '2026-05-06T12:00:00+02:00', lastErrorCode: null, lastErrorMessage: null }],
      backupStatuses: [{ sourcePath: 'D:\\CADProjects', backupRootPath: 'F:\\Backups', lastState: 'Success', lastSuccessAt: '2026-05-06T11:00:00+02:00', lastAttemptAt: '2026-05-06T11:00:00+02:00', lastErrorCode: null, lastErrorMessage: null }],
    });
  }
  if (String(url).startsWith('/api/runs/recent')) {
    return jsonResponse([{
      runId: 'run-123',
      taskType: 'Backup',
      state: 'Success',
      startedAt: '2026-05-06T12:00:00+02:00',
      finishedAt: '2026-05-06T12:01:00+02:00',
      summary: 'Completed',
      totalBytes: 100,
      copiedBytes: 100,
      sources: ['D:\\CADProjects'],
      targets: ['F:\\Backups'],
      partiallySkippedFiles: [],
      errors: [],
    }]);
  }
  if (String(url).startsWith('/api/actions/')) {
    return jsonResponse({ ok: true, message: 'Scheduled', runId: 'run-123' });
  }
  if (url === '/api/update/check') {
    return jsonResponse({
      currentVersion: '0.1.0',
      latestVersion: '0.2.0',
      runtime: 'win-x64',
      updateAvailable: true,
      installerUrl: 'https://updates.example.test/abds/ABDS-Setup-0.2.0-win-x64.exe',
      releaseNotes: 'Smoke update',
      message: 'Update available.',
    });
  }
  if (url === '/api/update/install') {
    return jsonResponse({ started: true, installerPath: 'C:\\Temp\\ABDS-Setup.exe', message: 'Installer started' });
  }
  if (String(url).startsWith('/api/runs/run-123/logs')) {
    return jsonResponse([{ at: '2026-05-06T12:00:00+02:00', level: 'INFO', message: 'Started' }]);
  }
  if (String(url).startsWith('/api/runs/run-123')) {
    return jsonResponse({
      runId: 'run-123',
      taskType: 'Sync',
      state: 'Running',
      startedAt: '2026-05-06T12:00:00+02:00',
      finishedAt: null,
      summary: null,
      totalBytes: 100,
      copiedBytes: 50,
      sources: ['D:\\CADProjects'],
      targets: ['E:\\Sync1'],
      partiallySkippedFiles: [],
      errors: [],
    });
  }
  return jsonResponse({});
};

window.eval(script);
await settle();

assert.equal(window.document.querySelector('[data-panel="status"]').classList.contains('hidden'), false);
assert.match(window.document.body.textContent, /Status systemu/);

click('[data-tab="sync"]');
assert.equal(window.document.querySelector('[data-panel="sync"]').classList.contains('hidden'), false);
assert.match(window.document.body.textContent, /D:\\CADProjects/);
assert.equal(window.document.querySelector('#syncFilter'), null);
click('[data-sync-status-filter="errors"]');
assert.match(window.document.body.textContent, /No sync pairs|Brak par synchronizacji/);
click('[data-sync-status-filter="all"]');
assert.match(window.document.body.textContent, /D:\\CADProjects/);

window.document.querySelector('#languageSelect').value = 'en';
window.document.querySelector('#languageSelect').dispatchEvent(new window.Event('change', { bubbles: true }));
await settle();
assert.match(window.document.body.textContent, /Synchronization/);
assert.match(window.document.body.textContent, /Sync now/);

window.document.querySelector('#themeSelect').value = 'dark';
window.document.querySelector('#themeSelect').dispatchEvent(new window.Event('change', { bubbles: true }));
assert.equal(window.document.documentElement.classList.contains('dark'), true);
window.document.querySelector('#themeSelect').value = 'light';
window.document.querySelector('#themeSelect').dispatchEvent(new window.Event('change', { bubbles: true }));
assert.equal(window.document.documentElement.classList.contains('dark'), false);

assert.equal(window.document.querySelector('[data-copy-path]'), null);
click('[data-header-action="sync-add"]');
assert.equal(window.document.querySelector('#editModal').open, true);
click('[data-browse-path]');
await settle();
assert.equal(window.document.querySelector('[name="sourcePath"]').value, 'D:\\PickedFolder');
window.document.querySelector('#editModal').close();

click('#syncAllBtn');
await settle();
assert(fetchCalls.some((call) => call.url === '/api/actions/sync/all' && call.method === 'POST'));
assert.equal(window.document.querySelector('#taskModal').open, false);

click('[data-header-action="sync-add"]');
assert.equal(window.document.querySelector('#editModal').open, true);
window.document.querySelector('[name="sourcePath"]').value = 'D:\\NewSource';
window.document.querySelector('[name="targetPaths"]').value = 'E:\\NewTarget';
window.document.querySelector('#editModalBody').dispatchEvent(new window.Event('submit', { bubbles: true, cancelable: true }));
await settle();
assert(fetchCalls.some((call) => call.url === '/api/config' && call.method === 'PUT'));
assert.match(window.localStorage.getItem('abds-recent-paths'), /D:\\\\NewSource/);

click('[data-tab="settings"]');
click('[data-update-action="check"]');
await settle();
assert.match(window.document.body.textContent, /0\.2\.0/);
click('[data-update-action="install"]');
await settle();
assert(fetchCalls.some((call) => call.url === '/api/update/install' && call.method === 'POST'));
click('[data-header-action="settings-save"]');
await settle();
assert(fetchCalls.filter((call) => call.url === '/api/config' && call.method === 'PUT').length >= 2);
assert(fetchCalls.some((call) => call.url === '/api/windows/startup' && call.method === 'PUT'));
const settingsSave = fetchCalls.filter((call) => call.url === '/api/config' && call.method === 'PUT').at(-1);
assert.equal(JSON.parse(settingsSave.body).schedule.backupScheduleTime, '04:00');
assert.equal(JSON.parse(settingsSave.body).update.manifestUrl, 'https://abds.sokolowskifilip.pl/update/version');

console.log('web-ui smoke passed');
window.close();

function click(selector) {
  const node = window.document.querySelector(selector);
  assert(node, `Missing element: ${selector}`);
  node.dispatchEvent(new window.MouseEvent('click', { bubbles: true }));
}

function jsonResponse(body) {
  return Promise.resolve(new Response(JSON.stringify(body), {
    status: 200,
    headers: { 'content-type': 'application/json' },
  }));
}

async function settle() {
  await new Promise((resolve) => setTimeout(resolve, 25));
}
