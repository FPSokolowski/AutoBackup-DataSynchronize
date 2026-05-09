const dictionaries = {
  pl: {
    appSubtitle: 'local safety backup',
    common: {
      close: 'Zamknij',
      copy: 'Kopiuj',
      copied: 'Skopiowano do schowka',
      browse: 'Wskaż',
      browseFallback: 'Wklej ścieżkę folderu',
      retest: 'Test',
      save: 'Zapisz',
      cancel: 'Anuluj',
      edit: 'Edytuj',
      details: 'Szczegóły',
      add: 'Dodaj',
      run: 'Uruchom',
      enabled: 'Włączona',
      disabled: 'Wyłączona',
      noData: 'Brak danych',
      loading: 'Ładowanie...'
    },
    tabs: {
      status: ['Status', 'Status systemu'],
      sync: ['Synchronizacja', 'Pary źródło-cel'],
      backups: ['Backupy', 'Snapshoty i retencja'],
      actions: ['Akcje', 'Ręczne operacje'],
      history: ['Historia', 'Ostatnie zadania'],
      diagnostics: ['Diagnostyka', 'Usługa, IPC i ścieżki'],
      settings: ['Ustawienia', 'Harmonogram i zachowanie']
    },
    connection: {
      connecting: 'Sprawdzanie usługi',
      connected: 'Usługa lokalna aktywna',
      offline: 'Usługa niedostępna',
      offlineTitle: 'Usługa ABDS nie odpowiada',
      offlineBody: 'Panel działa, ale dane statusu i akcje wymagają uruchomionej usługi Windows.',
      pipe: 'Lokalne IPC',
      startService: 'Uruchom usługę',
      restartService: 'Restartuj usługę',
      startingService: 'Próba uruchomienia usługi...',
      restartingService: 'Próba restartu usługi...',
      serviceActionOk: 'Polecenie wysłane do usługi',
      serviceActionFailed: 'Nie udało się sterować usługą',
      notInstalled: 'Usługa nie jest zainstalowana'
    },
    actions: {
      syncNow: 'Synchronizuj teraz',
      backupNow: 'Backup teraz',
      syncAll: 'Synchronizuj wszystko',
      backupAll: 'Backup wszystkiego',
      cancelRun: 'Anuluj zadanie',
      actionFailed: 'Nie udało się wykonać akcji. Sprawdź, czy usługa ABDS działa.',
      queued: 'Zadanie dodane do kolejki',
      manualTitle: 'Akcje ręczne',
      manualDesc: 'ABDS wykonuje jedno zadanie naraz, aby uniknąć konfliktów na plikach.',
      syncDesc: 'Uruchom synchronizację wszystkich aktywnych par.',
      backupDesc: 'Utwórz snapshoty dla wszystkich aktywnych źródeł.'
    },
    theme: { system: 'System', dark: 'Ciemny', light: 'Jasny' },
    language: { label: 'Język', pl: 'PL', en: 'US' },
    status: {
      systemState: 'Stan systemu',
      lastSync: 'Ostatnia synchronizacja',
      lastBackup: 'Ostatni backup',
      issues: 'Problemy',
      noMessages: 'Brak komunikatów.',
      pairsInStatus: 'par w statusie',
      backupSources: 'źródeł backupu',
      needsReview: 'Wymagana weryfikacja',
      noIssues: 'Brak aktywnych problemów',
      activeTask: 'Aktywne zadanie',
      syncStatus: 'Status synchronizacji',
      backupStatus: 'Status backupów',
      issueCenter: 'Centrum problemów',
      firstRunHint: 'Po pierwszym uruchomieniu zadania pojawią się tu statusy.',
      noIssueTitle: 'Brak aktywnych problemów',
      noIssueBody: 'ABDS nie zgłasza błędów ani ostrzeżeń.'
    },
    destination: {
      available: 'Dostępna',
      unavailable: 'Niedostępna',
      unknown: 'Nieprzetestowana',
      writable: 'zapis OK',
      kind: 'Typ celu',
      source: 'Źródło',
      target: 'Cel',
      retestOk: 'Test lokalizacji zakończony powodzeniem',
      retestFailed: 'Test lokalizacji nie powiódł się'
    },
    table: {
      source: 'Źródło',
      target: 'Cel',
      backupRoot: 'Folder backupów',
      state: 'Stan',
      lastSuccess: 'Ostatni sukces',
      actions: 'Akcje'
    },
    sync: {
      title: 'Synchronizacja',
      desc: 'Zarządzanie parami źródło-cel i trybem porównywania plików.',
      addPair: 'Dodaj parę',
      all: 'Wszystko',
      errors: 'Błędy',
      inProgress: 'W toku',
      emptyTitle: 'Brak par synchronizacji',
      emptyBody: 'Dodaj pierwszą parę, aby ABDS zaczął synchronizować pliki.',
      targets: 'Cele',
      mode: 'Tryb',
      interval: 'Interwał',
      addTitle: 'Dodaj synchronizację',
      editTitle: 'Edytuj synchronizację',
      sourcePath: 'Folder źródłowy',
      targetPaths: 'Foldery docelowe'
    },
    backups: {
      title: 'Backupy',
      desc: 'Źródła backupu, foldery snapshotów i limit retencji.',
      addSource: 'Dodaj źródło',
      all: 'Backup wszystkich',
      list: 'Lista źródeł backupu',
      retention: 'Retencja',
      retentionDesc: 'Globalny limit danych backupu',
      limit: 'Limit',
      retentionHint: 'Po przekroczeniu limitu ABDS usuwa najstarsze snapshoty.',
      emptyTitle: 'Brak źródeł backupu',
      emptyBody: 'Dodaj folder źródłowy oraz katalog snapshotów.',
      addTitle: 'Dodaj backup',
      editTitle: 'Edytuj backup',
      name: 'Nazwa',
      duplicateName: 'Nazwa backupu musi być unikalna.',
      nameRequired: 'Nazwa backupu jest wymagana.',
      sourcePath: 'Folder źródłowy',
      rootPath: 'Folder backupów'
    },
    settings: {
      title: 'Ustawienia systemowe',
      desc: 'Konfiguracja harmonogramu, porównywania plików i retencji.',
      save: 'Zapisz ustawienia',
      defaults: 'Przywróć domyślne',
      schedule: 'Harmonogram',
      autoSync: 'Auto sync',
      syncInterval: 'Interwał synchronizacji',
      autoBackup: 'Auto backup',
      backupSchedule: 'Harmonogram backupu',
      backupScheduleMode: 'Tryb harmonogramu',
      weekly: 'Tydzień',
      monthly: 'Miesiąc',
      backupTime: 'Godzina backupu',
      weekdays: 'Dni tygodnia',
      monthDays: 'Dni miesiąca',
      compression: 'Kompresja backupu',
      archiveFormat: 'Format archiwum',
      compressionLevel: 'Poziom kompresji',
      compressionLevels: ['Brak kompresji', 'Najszybsza', 'Optymalna', 'Najmniejszy plik'],
      comparison: 'Porównywanie plików',
      comparisonMode: 'Tryb porównywania',
      hashThreshold: 'Hash poniżej (MB)',
      retentionStart: 'Retencja i start',
      maxBackupGb: 'Maksymalny rozmiar backupów (GB)',
      criticalBackupHours: 'Dodatkowy próg krytyczny backupu (h)',
      syncOnStart: 'Sync przy starcie aplikacji',
      syncOnExit: 'Sync przy wyjściu z aplikacji',
      startupWithWindows: 'Startuj razem z Windows',
      systemPaths: 'Ścieżki systemowe',
      saved: 'Ustawienia zapisane',
      weekdaysShort: ['Nd', 'Pn', 'Wt', 'Śr', 'Cz', 'Pt', 'Sb']
    },
    diagnostics: {
      title: 'Diagnostyka',
      desc: 'Stan lokalnego połączenia, IPC i plików w ProgramData.',
      windowsService: 'Usługa Windows',
      connected: 'Aktywna',
      ipc: 'IPC',
      dumps: 'Dumpy błędów'
    },
    history: {
      title: 'Historia',
      desc: 'Ostatnie zadania zapisane przez usługę ABDS.',
      emptyTitle: 'Brak historii',
      emptyBody: 'Po wykonaniu pierwszego zadania pojawią się tu wyniki.'
    },
    task: {
      detailsTitle: 'Szczegóły zadania',
      notFound: 'Nie znaleziono szczegółów zadania.',
      type: 'Typ',
      state: 'Stan',
      start: 'Start',
      finish: 'Koniec',
      progress: 'Postęp',
      sources: 'Źródła',
      targets: 'Cele',
      skipped: 'Pominięte pliki',
      logs: 'Logi'
    },
    states: {
      Busy: 'W toku',
      Ok: 'OK',
      Warning: 'Ostrzeżenie',
      Critical: 'Krytyczne',
      Running: 'W toku',
      Success: 'Sukces',
      Failed: 'Błąd',
      PartiallyDone: 'Częściowo',
      RetryWaiting: 'Ponowienie',
      Cancelled: 'Anulowano',
      Sync: 'Synchronizacja',
      Backup: 'Backup'
    },
    modes: ['Tylko metadane', 'Hash poniżej limitu', 'Hash wszystkiego']
  },
  en: {
    appSubtitle: 'local safety backup',
    common: {
      close: 'Close',
      copy: 'Copy',
      copied: 'Copied to clipboard',
      browse: 'Browse',
      browseFallback: 'Paste folder path',
      retest: 'Test',
      save: 'Save',
      cancel: 'Cancel',
      edit: 'Edit',
      details: 'Details',
      add: 'Add',
      run: 'Run',
      enabled: 'Enabled',
      disabled: 'Disabled',
      noData: 'No data',
      loading: 'Loading...'
    },
    tabs: {
      status: ['Status', 'System status'],
      sync: ['Synchronization', 'Source-target pairs'],
      backups: ['Backups', 'Snapshots and retention'],
      actions: ['Actions', 'Manual operations'],
      history: ['History', 'Recent jobs'],
      diagnostics: ['Diagnostics', 'Service, IPC and paths'],
      settings: ['Settings', 'Schedule and behavior']
    },
    connection: {
      connecting: 'Checking service',
      connected: 'Local service active',
      offline: 'Service unavailable',
      offlineTitle: 'ABDS service is not responding',
      offlineBody: 'The panel is running, but status data and actions require the Windows service.',
      pipe: 'Local IPC',
      startService: 'Start service',
      restartService: 'Restart service',
      startingService: 'Trying to start service...',
      restartingService: 'Trying to restart service...',
      serviceActionOk: 'Service command sent',
      serviceActionFailed: 'Could not control service',
      notInstalled: 'Service is not installed'
    },
    actions: {
      syncNow: 'Sync now',
      backupNow: 'Backup now',
      syncAll: 'Sync all',
      backupAll: 'Backup all',
      cancelRun: 'Cancel job',
      actionFailed: 'Action failed. Check whether the ABDS service is running.',
      queued: 'Job queued',
      manualTitle: 'Manual actions',
      manualDesc: 'ABDS runs one job at a time to avoid file conflicts.',
      syncDesc: 'Run synchronization for every enabled pair.',
      backupDesc: 'Create snapshots for every enabled backup source.'
    },
    theme: { system: 'System', dark: 'Dark', light: 'Light' },
    language: { label: 'Language', pl: 'PL', en: 'US' },
    status: {
      systemState: 'System state',
      lastSync: 'Last synchronization',
      lastBackup: 'Last backup',
      issues: 'Issues',
      noMessages: 'No messages.',
      pairsInStatus: 'pairs in status',
      backupSources: 'backup sources',
      needsReview: 'Review required',
      noIssues: 'No active issues',
      activeTask: 'Active job',
      syncStatus: 'Synchronization status',
      backupStatus: 'Backup status',
      issueCenter: 'Issue center',
      firstRunHint: 'Statuses will appear here after the first job runs.',
      noIssueTitle: 'No active issues',
      noIssueBody: 'ABDS reports no errors or warnings.'
    },
    destination: {
      available: 'Available',
      unavailable: 'Unavailable',
      unknown: 'Not tested',
      writable: 'write OK',
      kind: 'Destination type',
      source: 'Source',
      target: 'Target',
      retestOk: 'Destination test passed',
      retestFailed: 'Destination test failed'
    },
    table: {
      source: 'Source',
      target: 'Target',
      backupRoot: 'Backup folder',
      state: 'State',
      lastSuccess: 'Last success',
      actions: 'Actions'
    },
    sync: {
      title: 'Synchronization',
      desc: 'Manage source-target pairs and file comparison mode.',
      addPair: 'Add pair',
      all: 'All',
      errors: 'Errors',
      inProgress: 'Running',
      emptyTitle: 'No sync pairs',
      emptyBody: 'Add the first pair to start synchronizing files.',
      targets: 'Targets',
      mode: 'Mode',
      interval: 'Interval',
      addTitle: 'Add synchronization',
      editTitle: 'Edit synchronization',
      sourcePath: 'Source folder',
      targetPaths: 'Target folders'
    },
    backups: {
      title: 'Backups',
      desc: 'Backup sources, snapshot folders and retention limit.',
      addSource: 'Add source',
      all: 'Backup all',
      list: 'Backup source list',
      retention: 'Retention',
      retentionDesc: 'Global backup data limit',
      limit: 'Limit',
      retentionHint: 'When the limit is exceeded, ABDS deletes oldest snapshots.',
      emptyTitle: 'No backup sources',
      emptyBody: 'Add source folder and snapshot directory.',
      addTitle: 'Add backup',
      editTitle: 'Edit backup',
      name: 'Name',
      duplicateName: 'Backup name must be unique.',
      nameRequired: 'Backup name is required.',
      sourcePath: 'Source folder',
      rootPath: 'Backup folder'
    },
    settings: {
      title: 'System settings',
      desc: 'Configure schedule, comparison and retention.',
      save: 'Save settings',
      defaults: 'Restore defaults',
      schedule: 'Schedule',
      autoSync: 'Auto sync',
      syncInterval: 'Sync interval',
      autoBackup: 'Auto backup',
      backupSchedule: 'Backup schedule',
      backupScheduleMode: 'Schedule mode',
      weekly: 'Week',
      monthly: 'Month',
      backupTime: 'Backup time',
      weekdays: 'Weekdays',
      monthDays: 'Month days',
      compression: 'Backup compression',
      archiveFormat: 'Archive format',
      compressionLevel: 'Compression level',
      compressionLevels: ['No compression', 'Fastest', 'Optimal', 'Smallest file'],
      comparison: 'File comparison',
      comparisonMode: 'Comparison mode',
      hashThreshold: 'Hash below (MB)',
      retentionStart: 'Retention and startup',
      maxBackupGb: 'Maximum backup size (GB)',
      criticalBackupHours: 'Extra critical backup threshold (h)',
      syncOnStart: 'Sync on app start',
      syncOnExit: 'Sync on app exit',
      startupWithWindows: 'Start with Windows',
      systemPaths: 'System paths',
      saved: 'Settings saved',
      weekdaysShort: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']
    },
    diagnostics: {
      title: 'Diagnostics',
      desc: 'Local connection, IPC and ProgramData files.',
      windowsService: 'Windows service',
      connected: 'Active',
      ipc: 'IPC',
      dumps: 'Failure dumps'
    },
    history: {
      title: 'History',
      desc: 'Recent jobs saved by the ABDS service.',
      emptyTitle: 'No history yet',
      emptyBody: 'Completed jobs will appear here after the first run.'
    },
    task: {
      detailsTitle: 'Job details',
      notFound: 'Job details not found.',
      type: 'Type',
      state: 'State',
      start: 'Start',
      finish: 'Finish',
      progress: 'Progress',
      sources: 'Sources',
      targets: 'Targets',
      skipped: 'Skipped files',
      logs: 'Logs'
    },
    states: {
      Busy: 'Running',
      Ok: 'OK',
      Warning: 'Warning',
      Critical: 'Critical',
      Running: 'Running',
      Success: 'Success',
      Failed: 'Failed',
      PartiallyDone: 'Partial',
      RetryWaiting: 'Retry waiting',
      Cancelled: 'Cancelled',
      Sync: 'Synchronization',
      Backup: 'Backup'
    },
    modes: ['Metadata only', 'Hash below limit', 'Hash everything']
  }
};

const tabs = [
  { id: 'status', icon: 'dashboard' },
  { id: 'sync', icon: 'sync' },
  { id: 'backups', icon: 'backup' },
  { id: 'actions', icon: 'bolt' },
  { id: 'history', icon: 'history_toggle_off' },
  { id: 'diagnostics', icon: 'terminal' },
  { id: 'settings', icon: 'settings' }
];

const state = {
  activeTab: getPreference('abds-active-tab') || 'status',
  connected: false,
  status: null,
  config: null,
  paths: null,
  startup: null,
  recentRuns: [],
  currentRunId: null,
  syncStatusFilter: getPreference('abds-sync-status-filter') || 'all',
  language: getPreference('abds-language') || 'pl',
  theme: getPreference('abds-theme') || 'system'
};

const pathBrowseRequests = new Map();
const recentPathsKey = 'abds-recent-paths';

const els = {
  sideNav: document.getElementById('sideNav'),
  mobileTabs: document.getElementById('mobileTabs'),
  pageSubtitle: document.getElementById('pageSubtitle'),
  serviceBanner: document.getElementById('serviceBanner'),
  connectionBadge: document.getElementById('connectionBadge'),
  navStatusDot: document.getElementById('navStatusDot'),
  navStatusText: document.getElementById('navStatusText'),
  themeSelect: document.getElementById('themeSelect'),
  languageSelect: document.getElementById('languageSelect'),
  taskModal: document.getElementById('taskModal'),
  taskModalSubtitle: document.getElementById('taskModalSubtitle'),
  taskModalBody: document.getElementById('taskModalBody'),
  editModal: document.getElementById('editModal'),
  editModalTitle: document.getElementById('editModalTitle'),
  editModalBody: document.getElementById('editModalBody'),
  toastHost: document.getElementById('toastHost')
};

init();

async function init() {
  els.themeSelect.value = state.theme;
  els.languageSelect.value = state.language;
  document.documentElement.lang = state.language;
  renderNavigation();
  wireGlobalActions();
  applyTheme();
  applyTranslations();
  selectTab(state.activeTab, false);

  await Promise.allSettled([refreshConfig(), refreshPaths(), refreshStartup(), refreshRecentRuns(), refreshStatus()]);
  renderAll();

  const initialRunId = new URLSearchParams(window.location.search).get('runId');
  if (initialRunId) openTaskModal(initialRunId);

  setInterval(refreshStatus, 5000);
}

function wireGlobalActions() {
  document.body.addEventListener('click', handleDocumentClick);
  document.body.addEventListener('change', handleDocumentChange);
  document.body.addEventListener('input', handlePathInput);
  document.body.addEventListener('focusin', handlePathFocus);
  document.body.addEventListener('focusout', () => setTimeout(closePathSuggestions, 140));
  window.chrome?.webview?.addEventListener?.('message', handleHostMessage);
  els.themeSelect.addEventListener('change', () => {
    state.theme = els.themeSelect.value;
    setPreference('abds-theme', state.theme);
    applyTheme();
  });
  els.languageSelect.addEventListener('change', () => {
    state.language = els.languageSelect.value;
    setPreference('abds-language', state.language);
    document.documentElement.lang = state.language;
    applyTranslations();
    setConnection(state.connected);
    renderNavigation();
    renderAll();
  });
  window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', applyTheme);
  document.getElementById('syncAllBtn').addEventListener('click', () => runAction('/api/actions/sync/all'));
  document.getElementById('backupAllBtn').addEventListener('click', () => runAction('/api/actions/backup/all'));
}

function handleDocumentClick(event) {
  const target = event.target.closest('button');
  if (!target) return;

  if (target.dataset.tab) selectTab(target.dataset.tab);
  if (target.dataset.closeModal !== undefined) els.taskModal.close();
  if (target.dataset.closeEdit !== undefined) els.editModal.close();
  if (target.dataset.browsePath !== undefined) browseForPath(target);
  if (target.dataset.pathSuggestion) usePathSuggestion(target);
  if (target.dataset.openRun) openTaskModal(target.dataset.openRun);
  if (target.dataset.cancelRun) cancelRun(target.dataset.cancelRun);
  if (target.dataset.headerAction) handleHeaderAction(target.dataset.headerAction);
  if (target.dataset.syncStatusFilter) {
    state.syncStatusFilter = target.dataset.syncStatusFilter;
    setPreference('abds-sync-status-filter', state.syncStatusFilter);
    renderSyncPanel();
  }
  if (target.dataset.serviceAction) runServiceAction(target.dataset.serviceAction);
  if (target.dataset.retestSyncSource) retestSyncDestination(target.dataset.retestSyncSource, target.dataset.retestSyncTarget);
  if (target.dataset.retestDestination) retestDestination(target.dataset.retestDestination);
  if (target.dataset.runUrl) runAction(target.dataset.runUrl);
  if (target.dataset.editSync) openSyncEditor(Number(target.dataset.editSync));
  if (target.dataset.editBackup) openBackupEditor(Number(target.dataset.editBackup));
  if (target.classList.contains('row-details')) openObjectDetails(JSON.parse(target.dataset.details));
  if (target.classList.contains('row-run')) {
    const body = target.dataset.actionType === 'sync'
      ? { sourcePath: target.dataset.source, targetPath: target.dataset.target }
      : { sourcePath: target.dataset.source, backupRootPath: target.dataset.target };
    runAction(target.dataset.actionType === 'sync' ? '/api/actions/sync/pair' : '/api/actions/backup/source', body);
  }
}

function handleDocumentChange(event) {
  if (event.target?.name === 'backupScheduleMode') updateBackupScheduleVisibility();
}

function handleHeaderAction(action) {
  if (action === 'sync-add') openSyncEditor();
  if (action === 'sync-all') runAction('/api/actions/sync/all');
  if (action === 'backup-add') openBackupEditor();
  if (action === 'backup-all') runAction('/api/actions/backup/all');
  if (action === 'settings-save') saveSettings();
  if (action === 'settings-defaults') showToast(t('settings.defaults'));
}

function applyTranslations() {
  document.querySelectorAll('[data-i18n]').forEach((node) => {
    node.textContent = t(node.dataset.i18n);
  });
  document.querySelectorAll('[data-i18n-placeholder]').forEach((node) => {
    node.setAttribute('placeholder', t(node.dataset.i18nPlaceholder));
  });
  for (const option of els.themeSelect.options) {
    if (option.dataset.i18n) option.textContent = t(option.dataset.i18n);
  }
  document.title = 'ABDS System';
}

function renderNavigation() {
  els.sideNav.innerHTML = tabs.map(tabButton).join('');
  els.mobileTabs.innerHTML = tabs.map((tab) => tabButton(tab, true)).join('');
  setActiveTabClasses();
}

function tabButton(tab, mobile = false) {
  const [label] = t(`tabs.${tab.id}`);
  return `
    <button data-tab="${tab.id}" class="${mobile ? 'mobile-tab inline-flex shrink-0 items-center gap-2 rounded-md border border-outline px-3 py-2 text-sm' : 'nav-btn flex w-full items-center gap-3 rounded-md px-5 py-2.5 text-left text-sm transition'}">
      <span class="material-symbols-outlined text-[20px]">${tab.icon}</span>
      <span>${label}</span>
    </button>
  `;
}

function selectTab(tabId, render = true) {
  state.activeTab = tabs.some((tab) => tab.id === tabId) ? tabId : 'status';
  setPreference('abds-active-tab', state.activeTab);
  document.querySelectorAll('.panel').forEach((panel) => panel.classList.toggle('hidden', panel.dataset.panel !== state.activeTab));
  setActiveTabClasses();
  els.pageSubtitle.textContent = t(`tabs.${state.activeTab}`)[1];
  if (render) renderAll();
}

function setActiveTabClasses() {
  document.querySelectorAll('[data-tab]').forEach((button) => {
    const active = button.dataset.tab === state.activeTab;
    button.classList.toggle('bg-primary', active);
    button.classList.toggle('text-slate-950', active);
    button.classList.toggle('font-bold', active);
    button.classList.toggle('bg-surface', !active && button.classList.contains('mobile-tab'));
    button.classList.toggle('text-muted', !active && button.classList.contains('nav-btn'));
    button.classList.toggle('hover:bg-surface-high', !active);
  });
}

function applyTheme() {
  const dark = state.theme === 'dark' || (state.theme === 'system' && window.matchMedia('(prefers-color-scheme: dark)').matches);
  document.documentElement.classList.toggle('dark', dark);
  els.themeSelect.value = state.theme;
  notifyHostTheme(dark);
}

function notifyHostTheme(dark) {
  window.chrome?.webview?.postMessage?.({ type: 'abds-theme', dark });
}

function handleHostMessage(event) {
  const message = event.data;
  if (!message || message.type !== 'abds-folder-selected')
    return;

  const field = pathBrowseRequests.get(message.requestId);
  pathBrowseRequests.delete(message.requestId);
  if (!field || !message.path)
    return;

  applyPathValue(field, message.path);
}

function browseForPath(button) {
  const field = button.closest('[data-path-input-wrap]')?.querySelector('[data-path-input="true"]');
  if (!field)
    return;

  const requestId = crypto.randomUUID?.() || `${Date.now()}-${Math.random()}`;
  pathBrowseRequests.set(requestId, field);

  if (window.chrome?.webview?.postMessage) {
    window.chrome.webview.postMessage({
      type: 'abds-browse-folder',
      requestId,
      currentPath: getCurrentPathText(field)
    });
    return;
  }

  const manualPath = window.prompt(t('common.browseFallback'), getCurrentPathText(field));
  if (manualPath)
    applyPathValue(field, manualPath);
}

function handlePathInput(event) {
  const field = event.target.closest?.('[data-path-input="true"]');
  if (field)
    renderPathSuggestions(field);
}

function handlePathFocus(event) {
  const field = event.target.closest?.('[data-path-input="true"]');
  if (field)
    renderPathSuggestions(field);
}

function usePathSuggestion(button) {
  const field = button.closest('[data-path-input-wrap]')?.querySelector('[data-path-input="true"]');
  if (field)
    applyPathValue(field, button.dataset.pathSuggestion);
}

function applyPathValue(field, path) {
  const cleanPath = String(path || '').trim();
  if (!cleanPath)
    return;

  if (field.dataset.multiline === 'true') {
    const existing = field.value.split('\n').map(x => x.trim()).filter(Boolean);
    if (!existing.some(x => x.toLowerCase() === cleanPath.toLowerCase()))
      existing.push(cleanPath);
    field.value = existing.join('\n');
  }
  else {
    field.value = cleanPath;
  }

  rememberPaths([cleanPath]);
  field.dispatchEvent(new Event('input', { bubbles: true }));
  field.dispatchEvent(new Event('change', { bubbles: true }));
  renderPathSuggestions(field);
  field.focus();
}

function renderPathSuggestions(field) {
  const host = field.closest('[data-path-input-wrap]')?.querySelector('[data-path-suggestions]');
  if (!host)
    return;

  const term = getCurrentPathText(field).toLowerCase();
  const suggestions = getRecentPaths()
    .filter(path => !term || path.toLowerCase().includes(term))
    .slice(0, 8);

  host.classList.toggle('hidden', suggestions.length === 0);
  host.classList.toggle('flex', suggestions.length > 0);
  host.innerHTML = suggestions.map(path => `
    <button type="button" data-path-suggestion="${escapeAttr(path)}" class="max-w-full truncate rounded-md border border-outline bg-surface px-2 py-1 font-mono text-xs text-muted hover:bg-surface-high">
      ${escapeHtml(path)}
    </button>
  `).join('');
}

function closePathSuggestions() {
  document.querySelectorAll('[data-path-suggestions]').forEach(node => {
    node.classList.add('hidden');
    node.classList.remove('flex');
  });
}

function getCurrentPathText(field) {
  if (field.dataset.multiline !== 'true')
    return field.value.trim();

  const lines = field.value.split('\n');
  return (lines.at(-1) || '').trim();
}

function getRecentPaths() {
  try {
    const parsed = JSON.parse(localStorage.getItem(recentPathsKey) || '[]');
    return Array.isArray(parsed) ? parsed.filter(Boolean).map(String) : [];
  }
  catch {
    return [];
  }
}

function rememberPaths(paths) {
  const incoming = paths.map(path => String(path || '').trim()).filter(Boolean);
  if (!incoming.length)
    return;

  const combined = [...incoming, ...getRecentPaths()];
  const unique = [];
  for (const path of combined) {
    if (!unique.some(existing => existing.toLowerCase() === path.toLowerCase()))
      unique.push(path);
  }

  localStorage.setItem(recentPathsKey, JSON.stringify(unique.slice(0, 12)));
}

async function refreshStatus() {
  try {
    state.status = await fetchJson('/api/status');
    await refreshRecentRuns();
    state.connected = true;
    setConnection(true);
  } catch (error) {
    state.connected = false;
    setConnection(false, error.message);
  }
  renderAll();
}

async function runServiceAction(action) {
  const url = action === 'restart' ? '/api/service/restart' : '/api/service/start';
  showToast(action === 'restart' ? t('connection.restartingService') : t('connection.startingService'));
  try {
    const result = await fetchJson(url, { method: 'POST' });
    if (!result.installed) {
      showToast(result.message || t('connection.notInstalled'), 'danger');
      return;
    }
    if (result.status === 'Error') {
      showToast(result.message || t('connection.serviceActionFailed'), 'danger');
      return;
    }
    showToast(t('connection.serviceActionOk'));
    await new Promise(resolve => setTimeout(resolve, 1200));
    await refreshStatus();
  } catch {
    showToast(t('connection.serviceActionFailed'), 'danger');
  }
}

async function retestDestination(location) {
  showToast(t('common.loading'));
  try {
    const response = await fetchJson('/api/destinations/retest', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ location })
    });
    state.config = response.config || response.Config || state.config;
    const result = response.result || response.Result;
    showToast((result?.available && result?.writable) ? t('destination.retestOk') : t('destination.retestFailed'), (result?.available && result?.writable) ? 'ok' : 'danger');
    renderAll();
  } catch {
    showToast(t('destination.retestFailed'), 'danger');
  }
}

async function retestSyncDestination(sourcePath, targetPath) {
  showToast(t('common.loading'));
  try {
    const response = await fetchJson('/api/destinations/retest-sync', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ sourcePath, targetPath })
    });
    state.config = response.config || response.Config || state.config;
    const sourceResult = response.sourceResult || response.SourceResult;
    const targetResult = response.targetResult || response.TargetResult;
    const ok = Boolean(sourceResult?.available && targetResult?.available && targetResult?.writable);
    showToast(ok ? t('destination.retestOk') : t('destination.retestFailed'), ok ? 'ok' : 'danger');
    renderAll();
  } catch {
    showToast(t('destination.retestFailed'), 'danger');
  }
}

async function refreshConfig() {
  try {
    state.config = await fetchJson('/api/config');
  } catch {
    state.config = null;
  }
}

async function refreshPaths() {
  try {
    state.paths = await fetchJson('/api/diagnostics/paths');
  } catch {
    state.paths = null;
  }
}

async function refreshStartup() {
  try {
    state.startup = await fetchJson('/api/windows/startup');
  } catch {
    state.startup = null;
  }
}

async function refreshRecentRuns() {
  try {
    state.recentRuns = await fetchJson('/api/runs/recent?take=30');
  } catch {
    state.recentRuns = [];
  }
}

async function fetchJson(url, options) {
  const response = await fetch(url, options);
  if (!response.ok) throw new Error(await response.text());
  return response.json();
}

function renderAll() {
  renderStatusPanel();
  renderSyncPanel();
  renderBackupsPanel();
  renderActionsPanel();
  renderHistoryPanel();
  renderDiagnosticsPanel();
  renderSettingsPanel();
}

function renderStatusPanel() {
  const status = state.status || {};
  const severity = value(status, 'traySeverity', 'TraySeverity') || (state.connected ? 'Ok' : 'Warning');
  const runningRunId = value(status, 'runningRunId', 'RunningRunId');
  const hasRunningJob = Boolean(value(status, 'hasRunningJob', 'HasRunningJob'));
  const syncStatuses = value(status, 'syncStatuses', 'SyncStatuses') || [];
  const backupStatuses = value(status, 'backupStatuses', 'BackupStatuses') || [];
  const issues = collectIssues(syncStatuses, backupStatuses, severity);

  panelEl('status').innerHTML = `
    <div class="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
      ${metricCard(t('status.systemState'), translateSeverity(severity), value(status, 'trayTooltip', 'TrayTooltip') || t('status.noMessages'), severity)}
      ${metricCard(t('status.lastSync'), newestDate(syncStatuses, 'lastSuccessAt', 'LastSuccessAt'), `${syncStatuses.length} ${t('status.pairsInStatus')}`, 'Busy')}
      ${metricCard(t('status.lastBackup'), newestDate(backupStatuses, 'lastSuccessAt', 'LastSuccessAt'), `${backupStatuses.length} ${t('status.backupSources')}`, 'Ok')}
      ${metricCard(t('status.issues'), `${issues.length}`, issues.length ? t('status.needsReview') : t('status.noIssues'), issues.length ? 'Critical' : 'Ok')}
    </div>
    ${hasRunningJob ? activeTaskCard(runningRunId) : ''}
    <div class="grid gap-6 xl:grid-cols-12">
      <section class="xl:col-span-8 space-y-6">
        ${statusTable(t('status.syncStatus'), syncStatuses, 'sync')}
        ${statusTable(t('status.backupStatus'), backupStatuses, 'backup')}
      </section>
      <aside class="xl:col-span-4">
        <div class="rounded-lg border border-outline bg-surface p-5">
          <div class="mb-4 flex items-center justify-between">
            <h2 class="font-display text-lg font-semibold">${t('status.issueCenter')}</h2>
            <span class="rounded-md bg-danger/15 px-2 py-1 text-sm font-bold text-danger">${issues.length}</span>
          </div>
          <div class="space-y-3">
            ${issues.length ? issues.map(issueCard).join('') : emptyState('shield_check', t('status.noIssueTitle'), t('status.noIssueBody'))}
          </div>
        </div>
      </aside>
    </div>
  `;
}

function activeTaskCard(runId) {
  return `
    <section class="rounded-lg border border-secondary/40 bg-surface p-5 glow-secondary">
      <div class="flex flex-wrap items-center justify-between gap-4">
        <div class="flex items-center gap-3">
          <span class="material-symbols-outlined animate-spin text-secondary">sync</span>
          <div>
            <h2 class="font-display text-xl font-semibold">${t('status.activeTask')}</h2>
            <p class="font-mono text-xs text-muted">RunId: ${escapeHtml(runId || '-')}</p>
          </div>
        </div>
        <div class="flex gap-2">
          <button data-open-run="${escapeAttr(runId || '')}" class="rounded-md border border-outline px-3 py-2 text-sm">${t('common.details')}</button>
          <button data-cancel-run="${escapeAttr(runId || '')}" class="rounded-md border border-danger px-3 py-2 text-sm text-danger">${t('actions.cancelRun')}</button>
        </div>
      </div>
      <div class="mt-5 h-2 overflow-hidden rounded-full bg-surface-high">
        <div class="h-full w-1/2 bg-secondary"></div>
      </div>
    </section>
  `;
}

function statusTable(title, rows, type) {
  const headers = type === 'sync'
    ? [t('table.source'), t('table.target'), t('table.state'), t('table.lastSuccess'), t('table.actions')]
    : [t('table.source'), t('table.backupRoot'), t('table.state'), t('table.lastSuccess'), t('table.actions')];
  return `
    <section class="overflow-hidden rounded-lg border border-outline bg-surface">
      <div class="border-b border-outline bg-surface-low px-5 py-4">
        <h2 class="font-display text-lg font-semibold">${title}</h2>
      </div>
      <div class="overflow-x-auto">
        <table class="w-full min-w-[820px] text-left text-sm">
          <thead class="border-b border-outline text-xs text-muted">
            <tr>${headers.map((h) => `<th class="px-5 py-3 font-semibold">${h}</th>`).join('')}</tr>
          </thead>
          <tbody class="divide-y divide-outline">
            ${rows.length ? rows.map((row) => statusRow(row, type)).join('') : `<tr><td colspan="5">${emptyState('inbox', t('common.noData'), t('status.firstRunHint'))}</td></tr>`}
          </tbody>
        </table>
      </div>
    </section>
  `;
}

function statusRow(row, type) {
  const source = value(row, 'sourcePath', 'SourcePath');
  const target = type === 'sync' ? value(row, 'targetPath', 'TargetPath') : value(row, 'backupRootPath', 'BackupRootPath');
  return `
    <tr class="hover:bg-surface-low">
      <td class="px-5 py-4">${pathChip(source)}</td>
      <td class="px-5 py-4">${pathChip(target)}</td>
      <td class="px-5 py-4">${stateBadge(value(row, 'lastState', 'LastState') || '-')}</td>
      <td class="px-5 py-4 text-muted">${formatDate(value(row, 'lastSuccessAt', 'LastSuccessAt'))}</td>
      <td class="px-5 py-4">
        <div class="flex gap-2">
          <button data-action-type="${type}" data-source="${escapeAttr(source)}" data-target="${escapeAttr(target)}" class="row-run rounded-md border border-outline px-2 py-1 text-xs">${type === 'sync' ? 'Sync' : 'Backup'}</button>
          <button data-details='${escapeAttr(JSON.stringify(row))}' class="row-details rounded-md border border-outline px-2 py-1 text-xs">${t('common.details')}</button>
        </div>
      </td>
    </tr>
  `;
}

function renderSyncPanel() {
  const config = state.config || {};
  const pairs = config.syncPairs || [];
  const filteredPairs = pairs.filter((pair) => {
    if (state.syncStatusFilter === 'enabled') return Boolean(pair.enabled);
    if (state.syncStatusFilter === 'errors') return pairHasSyncError(pair);
    return true;
  });
  panelEl('sync').innerHTML = `
    ${sectionHeader(t('sync.title'), t('sync.desc'), [['add', t('sync.addPair'), 'sync-add'], ['sync', t('actions.syncAll'), 'sync-all']])}
    <div class="rounded-lg border border-outline bg-surface">
      <div class="flex flex-wrap items-center justify-end gap-3 border-b border-outline bg-surface-low p-4">
        <div class="flex rounded-md border border-outline bg-background p-1 text-sm">
          ${syncFilterButton('all', t('sync.all'))}
          ${syncFilterButton('enabled', t('common.enabled'))}
          ${syncFilterButton('errors', t('sync.errors'))}
        </div>
      </div>
      <div class="space-y-4 bg-surface-low p-4">
        ${filteredPairs.length ? filteredPairs.map((pair) => syncPairCard(pair, pairs.indexOf(pair))).join('') : emptyState('sync_disabled', t('sync.emptyTitle'), t('sync.emptyBody'))}
      </div>
    </div>
  `;
}

function syncFilterButton(filter, label) {
  const active = state.syncStatusFilter === filter;
  return `<button data-sync-status-filter="${filter}" class="rounded px-3 py-1 font-semibold ${active ? 'bg-secondary text-slate-950' : 'text-muted hover:bg-surface-high'}">${label}</button>`;
}

function syncPairCard(pair, index) {
  return `
    <article class="grid gap-4 rounded-lg border border-outline bg-surface p-5 shadow-sm ring-1 ring-outline/40 lg:grid-cols-[1fr_auto]">
      <div class="min-w-0">
        <div class="flex flex-wrap items-center gap-3">
          ${togglePill(pair.enabled)}
          ${pathChip(pair.sourcePath)}
        </div>
        <div class="mt-3 grid gap-3 md:grid-cols-3">
          ${infoBlock(t('sync.targets'), String((pair.targetPaths || []).length))}
          ${infoBlock(t('sync.mode'), translateMode(state.config?.schedule?.syncComparisonMode))}
          ${infoBlock(t('sync.interval'), formatDuration(state.config?.schedule?.autoSyncInterval))}
        </div>
        <div class="mt-3 flex flex-wrap gap-2">
          ${(pair.targetPaths || []).map((path) => pathChip(path)).join('')}
        </div>
        <div class="mt-3 space-y-2">
          ${(pair.targetPaths || []).map((path) => syncDestinationStatusLine(pair, path)).join('')}
        </div>
      </div>
      <div class="flex items-start gap-2">
        <button data-edit-sync="${index}" class="rounded-md border border-outline px-3 py-2 text-sm">${t('common.edit')}</button>
        <button data-action-type="sync" data-source="${escapeAttr(pair.sourcePath)}" data-target="${escapeAttr((pair.targetPaths || [])[0] || '')}" class="row-run rounded-md bg-primary px-3 py-2 text-sm font-bold text-slate-950">${t('actions.syncNow')}</button>
      </div>
    </article>
  `;
}

function renderBackupsPanel() {
  const config = state.config || {};
  const backups = config.backupSources || [];
  panelEl('backups').innerHTML = `
    ${sectionHeader(t('backups.title'), t('backups.desc'), [['add', t('backups.addSource'), 'backup-add'], ['play_arrow', t('backups.all'), 'backup-all']])}
    <div class="grid gap-6 xl:grid-cols-12">
      <section class="xl:col-span-8 rounded-lg border border-outline bg-surface">
        <div class="border-b border-outline bg-surface-low px-5 py-4 font-display text-lg font-semibold">${t('backups.list')}</div>
        <div class="divide-y divide-outline">
          ${backups.length ? backups.map(backupCard).join('') : emptyState('backup', t('backups.emptyTitle'), t('backups.emptyBody'))}
        </div>
      </section>
      <aside class="xl:col-span-4 rounded-lg border border-outline bg-surface p-5">
        <div class="flex items-start justify-between">
          <div>
            <h2 class="font-display text-lg font-semibold">${t('backups.retention')}</h2>
            <p class="text-sm text-muted">${t('backups.retentionDesc')}</p>
          </div>
          <span class="material-symbols-outlined text-warning">database</span>
        </div>
        <div class="mt-5">
          <div class="mb-2 flex justify-between text-sm"><span>${t('backups.limit')}</span><span class="font-mono text-primary">${bytesToGb(config.schedule?.maxBackupStorageBytes)} GB</span></div>
          <div class="h-3 overflow-hidden rounded-full bg-surface-high"><div class="h-full w-3/4 bg-primary"></div></div>
          <p class="mt-3 text-sm text-muted">${t('backups.retentionHint')}</p>
        </div>
      </aside>
    </div>
  `;
}

function backupCard(backup, index) {
  return `
    <article class="grid gap-4 p-5 lg:grid-cols-[1fr_auto]">
      <div>
        <div class="flex flex-wrap items-center gap-3">${togglePill(backup.enabled)}${pathChip(backup.sourcePath)}</div>
        <div class="mt-3 grid gap-3 md:grid-cols-3">
          ${infoBlock(t('backups.name'), backup.name || backup.Name || '-')}
          ${infoBlock(t('table.backupRoot'), backup.backupRootPath, true)}
          ${infoBlock(t('settings.backupSchedule'), backupScheduleSummary(state.config?.schedule))}
          ${infoBlock(t('backups.limit'), `${bytesToGb(state.config?.schedule?.maxBackupStorageBytes)} GB`)}
        </div>
        <div class="mt-3">${destinationStatusLine(backup.backupRootPath, backup.backupDestination)}</div>
      </div>
      <div class="flex items-start gap-2">
        <button data-edit-backup="${index}" class="rounded-md border border-outline px-3 py-2 text-sm">${t('common.edit')}</button>
        <button data-action-type="backup" data-source="${escapeAttr(backup.sourcePath)}" data-target="${escapeAttr(backup.backupRootPath)}" class="row-run rounded-md bg-secondary px-3 py-2 text-sm font-bold text-slate-950">${t('actions.backupNow')}</button>
      </div>
    </article>
  `;
}

function renderSettingsPanel() {
  const config = state.config || {};
  const schedule = config.schedule || {};
  panelEl('settings').innerHTML = `
    ${sectionHeader(t('settings.title'), t('settings.desc'), [['save', t('settings.save'), 'settings-save'], ['restart_alt', t('settings.defaults'), 'settings-defaults']])}
    <form id="settingsForm" class="grid gap-6 xl:grid-cols-12">
      ${settingsCard(t('settings.schedule'), 'schedule', `
        ${toggleField(t('settings.autoSync'), 'autoSyncEnabled', schedule.autoSyncEnabled ?? true)}
        ${textField(t('settings.syncInterval'), 'autoSyncInterval', schedule.autoSyncInterval || '00:15:00')}
        ${toggleField(t('settings.autoBackup'), 'autoBackupEnabled', schedule.autoBackupEnabled ?? true)}
        ${backupScheduleFields(schedule)}
      `, 'xl:col-span-7')}
      ${settingsCard(t('settings.comparison'), 'compare_arrows', `
        <label class="block text-sm font-semibold text-muted">${t('settings.comparisonMode')}</label>
        <select name="syncComparisonMode" class="mt-2 w-full rounded-md border-outline bg-background">
          ${modeOption(0, t('modes')[0], schedule.syncComparisonMode)}
          ${modeOption(1, t('modes')[1], schedule.syncComparisonMode)}
          ${modeOption(2, t('modes')[2], schedule.syncComparisonMode)}
        </select>
        ${textField(t('settings.hashThreshold'), 'hashBelowSizeMbThreshold', schedule.hashBelowSizeMbThreshold ?? 20)}
      `, 'xl:col-span-5')}
      ${settingsCard(t('settings.compression'), 'inventory_2', `
        <label class="block text-sm font-semibold text-muted">${t('settings.archiveFormat')}</label>
        <select name="backupArchiveFormat" class="mt-2 w-full rounded-md border-outline bg-background">
          ${option(0, 'ZIP', schedule.backupArchiveFormat, 0)}
          ${option(1, 'TAR.GZ', schedule.backupArchiveFormat, 0)}
        </select>
        <label class="block text-sm font-semibold text-muted">${t('settings.compressionLevel')}</label>
        <select name="backupCompressionPreset" class="mt-2 w-full rounded-md border-outline bg-background">
          ${option(0, t('settings.compressionLevels')[0], schedule.backupCompressionPreset, 2)}
          ${option(1, t('settings.compressionLevels')[1], schedule.backupCompressionPreset, 2)}
          ${option(2, t('settings.compressionLevels')[2], schedule.backupCompressionPreset, 2)}
          ${option(3, t('settings.compressionLevels')[3], schedule.backupCompressionPreset, 2)}
        </select>
      `, 'xl:col-span-5')}
      ${settingsCard(t('settings.retentionStart'), 'storage', `
        ${textField(t('settings.maxBackupGb'), 'maxBackupStorageGb', bytesToGb(schedule.maxBackupStorageBytes))}
        ${textField(t('settings.criticalBackupHours'), 'criticalBackupOverdueExtraHours', durationToHours(config.criticalBackupOverdueExtra))}
        ${toggleField(t('settings.syncOnStart'), 'syncOnAppStart', schedule.syncOnAppStart ?? true)}
        ${toggleField(t('settings.syncOnExit'), 'syncOnAppExit', schedule.syncOnAppExit ?? false)}
        ${toggleField(t('settings.startupWithWindows'), 'startupWithWindows', state.startup?.enabled ?? false)}
      `, 'xl:col-span-6')}
      ${settingsCard(t('settings.systemPaths'), 'folder_open', `
        ${pathLine('config.json', state.paths?.configPath)}
        ${pathLine('state.json', state.paths?.statePath)}
        ${pathLine('hashcache.json', state.paths?.hashCachePath)}
        ${pathLine('Dumps', state.paths?.dumpsDir)}
      `, 'xl:col-span-6')}
    </form>
  `;
  updateBackupScheduleVisibility();
}

function renderActionsPanel() {
  panelEl('actions').innerHTML = `
    ${sectionHeader(t('actions.manualTitle'), t('actions.manualDesc'), [])}
    <div class="grid gap-4 md:grid-cols-2">
      ${actionCard('sync', t('actions.syncAll'), t('actions.syncDesc'), '/api/actions/sync/all')}
      ${actionCard('backup', t('actions.backupAll'), t('actions.backupDesc'), '/api/actions/backup/all')}
    </div>
  `;
}

function renderHistoryPanel() {
  const runs = state.recentRuns || [];
  panelEl('history').innerHTML = `
    ${sectionHeader(t('history.title'), t('history.desc'), [])}
    ${runs.length ? `
      <div class="overflow-hidden rounded-lg border border-outline bg-surface">
        ${runs.map(run => `
          <article class="grid gap-3 border-b border-outline p-4 last:border-b-0 md:grid-cols-[1fr_auto]">
            <div>
              <div class="flex flex-wrap items-center gap-2">
                ${stateBadge(value(run, 'state', 'State'))}
                <span class="font-semibold">${translateTaskType(value(run, 'taskType', 'TaskType'))}</span>
                <span class="font-mono text-xs text-muted">${formatDate(value(run, 'finishedAt', 'FinishedAt') || value(run, 'startedAt', 'StartedAt'))}</span>
              </div>
              <div class="mt-2 font-mono text-xs text-muted">${escapeHtml((value(run, 'sources', 'Sources') || []).join(', ') || value(run, 'runId', 'RunId'))}</div>
            </div>
            <button data-open-run="${escapeAttr(value(run, 'runId', 'RunId'))}" class="rounded-md border border-outline px-3 py-2 text-sm">${t('common.details')}</button>
          </article>
        `).join('')}
      </div>
    ` : `
      <div class="rounded-lg border border-outline bg-surface p-6">
        ${emptyState('history_toggle_off', t('history.emptyTitle'), t('history.emptyBody'))}
      </div>
    `}
  `;
}

function renderDiagnosticsPanel() {
  panelEl('diagnostics').innerHTML = `
    ${sectionHeader(t('diagnostics.title'), t('diagnostics.desc'), [])}
    <div class="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
      ${diagnosticCard(t('diagnostics.windowsService'), state.connected ? t('diagnostics.connected') : t('connection.offline'), state.connected ? 'Ok' : 'Critical')}
      ${diagnosticCard(t('diagnostics.ipc'), 'ABDS_PIPE_V1', state.connected ? 'Ok' : 'Warning')}
      ${diagnosticCard('config.json', state.paths?.configPath || '-', 'Ok')}
      ${diagnosticCard(t('diagnostics.dumps'), state.paths?.dumpsDir || '-', 'Warning')}
    </div>
  `;
}

async function runAction(url, body = null) {
  try {
    const result = await fetchJson(url, {
      method: 'POST',
      headers: body ? { 'Content-Type': 'application/json' } : undefined,
      body: body ? JSON.stringify(body) : undefined
    });
    showToast(t('actions.queued'));
    const runId = value(result, 'runId', 'RunId');
    if (runId) state.currentRunId = runId;
    await refreshStatus();
  } catch {
    showToast(t('actions.actionFailed'), 'danger');
  }
}

async function cancelRun(runId) {
  if (!runId) return;
  await fetch(`/api/runs/${encodeURIComponent(runId)}/cancel`, { method: 'POST' });
  await refreshStatus();
}

async function openTaskModal(runId) {
  els.taskModalSubtitle.textContent = `RunId: ${runId}`;
  els.taskModalBody.innerHTML = `<div class="text-sm text-muted">${t('common.loading')}</div>`;
  els.taskModal.showModal();
  try {
    const [run, logs] = await Promise.all([
      fetchJson(`/api/runs/${encodeURIComponent(runId)}`),
      fetchJson(`/api/runs/${encodeURIComponent(runId)}/logs`)
    ]);
    els.taskModalBody.innerHTML = renderRunDetails(run, logs);
  } catch {
    els.taskModalBody.innerHTML = `<div class="text-danger">${t('task.notFound')}</div>`;
  }
}

function openObjectDetails(obj) {
  els.taskModalSubtitle.textContent = t('common.details');
  els.taskModalBody.innerHTML = `<pre class="overflow-auto rounded-lg bg-background p-4 font-mono text-xs">${escapeHtml(JSON.stringify(obj, null, 2))}</pre>`;
  els.taskModal.showModal();
}

function renderRunDetails(run, logs) {
  const total = Math.max(1, value(run, 'totalBytes', 'TotalBytes') || 0);
  const copied = value(run, 'copiedBytes', 'CopiedBytes') || 0;
  const progress = Math.min(100, Math.round((copied / total) * 100));
  const skipped = value(run, 'partiallySkippedFiles', 'PartiallySkippedFiles') || [];
  return `
    <div class="space-y-5">
      <div class="grid gap-4 md:grid-cols-4">
        ${smallStat(t('task.type'), translateTaskType(value(run, 'taskType', 'TaskType')))}
        ${smallStat(t('task.state'), translateState(value(run, 'state', 'State')))}
        ${smallStat(t('task.start'), formatDate(value(run, 'startedAt', 'StartedAt')))}
        ${smallStat(t('task.finish'), formatDate(value(run, 'finishedAt', 'FinishedAt')))}
      </div>
      <div>
        <div class="mb-2 flex justify-between text-sm"><span>${t('task.progress')}</span><span>${progress}%</span></div>
        <div class="h-3 overflow-hidden rounded-full bg-surface-high"><div class="h-full bg-secondary" style="width:${progress}%"></div></div>
        <p class="mt-2 font-mono text-xs text-muted">${formatBytes(copied)} / ${formatBytes(value(run, 'totalBytes', 'TotalBytes') || 0)}</p>
      </div>
      <div class="grid gap-4 md:grid-cols-2">
        ${pathList(t('task.sources'), value(run, 'sources', 'Sources') || [])}
        ${pathList(t('task.targets'), value(run, 'targets', 'Targets') || [])}
      </div>
      ${skipped.length ? pathList(t('task.skipped'), skipped, 'text-warning') : ''}
      <div>
        <h3 class="mb-2 font-semibold">${t('task.logs')}</h3>
        <pre class="max-h-80 overflow-auto rounded-lg border border-outline bg-background p-4 font-mono text-xs text-content">${escapeHtml((logs || []).map(formatLogLine).join('\n'))}</pre>
      </div>
    </div>
  `;
}

async function saveSettings() {
  if (!state.config) return;
  const form = document.getElementById('settingsForm');
  const data = new FormData(form);
  const config = cloneJson(state.config);
  config.schedule ??= {};
  config.schedule.autoSyncEnabled = data.get('autoSyncEnabled') === 'on';
  config.schedule.autoBackupEnabled = data.get('autoBackupEnabled') === 'on';
  config.schedule.syncOnAppStart = data.get('syncOnAppStart') === 'on';
  config.schedule.syncOnAppExit = data.get('syncOnAppExit') === 'on';
  config.schedule.autoSyncInterval = String(data.get('autoSyncInterval') || '00:15:00');
  config.schedule.backupScheduleMode = String(data.get('backupScheduleMode') || 'weekly');
  config.schedule.backupScheduleTime = String(data.get('backupScheduleTime') || '04:00');
  config.schedule.backupScheduleWeekDays = data.getAll('backupScheduleWeekDays').map(Number).filter(Number.isFinite);
  config.schedule.backupScheduleMonthDays = data.getAll('backupScheduleMonthDays').map(Number).filter(Number.isFinite);
  if (!config.schedule.backupScheduleWeekDays.length) config.schedule.backupScheduleWeekDays = [1];
  if (!config.schedule.backupScheduleMonthDays.length) config.schedule.backupScheduleMonthDays = [1];
  config.schedule.autoBackupIntervalFromLastSuccess ??= '12:00:00';
  config.schedule.backupArchiveFormat = Number(data.get('backupArchiveFormat') || 0);
  config.schedule.backupCompressionPreset = Number(data.get('backupCompressionPreset') ?? 2);
  config.schedule.syncComparisonMode = Number(data.get('syncComparisonMode') || 1);
  config.schedule.hashBelowSizeMbThreshold = Number(data.get('hashBelowSizeMbThreshold') || 20);
  config.schedule.maxBackupStorageBytes = Number(data.get('maxBackupStorageGb') || 300) * 1024 * 1024 * 1024;
  config.criticalBackupOverdueExtra = `${String(data.get('criticalBackupOverdueExtraHours') || 2).padStart(2, '0')}:00:00`;
  state.config = await fetchJson('/api/config', { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(config) });
  await saveStartupSetting(data.get('startupWithWindows') === 'on');
  showToast(t('settings.saved'));
  renderSettingsPanel();
}

async function saveStartupSetting(enabled) {
  state.startup = await fetchJson('/api/windows/startup', {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ enabled })
  });
}

function openSyncEditor(index = null) {
  state.config ??= { syncPairs: [], backupSources: [], schedule: {} };
  const pair = index === null ? { sourcePath: '', targetPaths: [''], enabled: true } : state.config.syncPairs[index];
  els.editModalTitle.textContent = index === null ? t('sync.addTitle') : t('sync.editTitle');
  els.editModalBody.innerHTML = `
    ${textInput(t('sync.sourcePath'), 'sourcePath', pair.sourcePath, false, true)}
    ${textInput(t('sync.targetPaths'), 'targetPaths', (pair.targetPaths || []).join('\n'), true, true)}
    ${checkInput(t('common.enabled'), 'enabled', pair.enabled)}
    ${modalButtons()}
  `;
  els.editModalBody.onsubmit = async (event) => {
    event.preventDefault();
    const form = new FormData(els.editModalBody);
    const next = { sourcePath: String(form.get('sourcePath') || ''), targetPaths: String(form.get('targetPaths') || '').split('\n').map(x => x.trim()).filter(Boolean), enabled: form.get('enabled') === 'on' };
    rememberPaths([next.sourcePath, ...next.targetPaths]);
    state.config.syncPairs ??= [];
    if (index === null) state.config.syncPairs.push(next);
    else state.config.syncPairs[index] = next;
    await saveConfigOnly();
    els.editModal.close();
    renderSyncPanel();
  };
  els.editModal.showModal();
}

function openBackupEditor(index = null) {
  state.config ??= { syncPairs: [], backupSources: [], schedule: {} };
  const backup = index === null ? { name: '', sourcePath: '', backupRootPath: '', enabled: true } : state.config.backupSources[index];
  els.editModalTitle.textContent = index === null ? t('backups.addTitle') : t('backups.editTitle');
  els.editModalBody.innerHTML = `
    ${textInput(t('backups.name'), 'name', backup.name || backup.Name || '')}
    ${textInput(t('backups.sourcePath'), 'sourcePath', backup.sourcePath, false, true)}
    ${textInput(t('backups.rootPath'), 'backupRootPath', backup.backupRootPath, false, true)}
    ${checkInput(t('common.enabled'), 'enabled', backup.enabled)}
    ${modalButtons()}
  `;
  els.editModalBody.onsubmit = async (event) => {
    event.preventDefault();
    const form = new FormData(els.editModalBody);
    const next = { name: String(form.get('name') || '').trim(), sourcePath: String(form.get('sourcePath') || ''), backupRootPath: String(form.get('backupRootPath') || ''), enabled: form.get('enabled') === 'on' };
    if (!next.name) {
      showToast(t('backups.nameRequired'), 'danger');
      return;
    }
    if (!isBackupNameUnique(next.name, index)) {
      showToast(t('backups.duplicateName'), 'danger');
      return;
    }
    rememberPaths([next.sourcePath, next.backupRootPath]);
    state.config.backupSources ??= [];
    if (index === null) state.config.backupSources.push(next);
    else state.config.backupSources[index] = next;
    await saveConfigOnly();
    els.editModal.close();
    renderBackupsPanel();
  };
  els.editModal.showModal();
}

async function saveConfigOnly() {
  state.config = await fetchJson('/api/config', { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(state.config) });
  showToast(t('settings.saved'));
}

function showToast(message, tone = 'ok') {
  const toast = document.createElement('div');
  toast.className = `rounded-lg border px-4 py-3 text-sm shadow-lg ${tone === 'danger' ? 'border-danger bg-danger text-slate-950 dark:text-slate-950' : 'border-primary bg-primary text-slate-950'}`;
  toast.textContent = message;
  els.toastHost.appendChild(toast);
  setTimeout(() => {
    toast.style.opacity = '0';
    toast.style.transition = 'opacity 250ms ease';
    setTimeout(() => toast.remove(), 300);
  }, 2600);
}

function setConnection(connected) {
  els.serviceBanner.classList.toggle('hidden', connected);
  els.connectionBadge.innerHTML = `<span class="h-2 w-2 rounded-full ${connected ? 'bg-primary' : 'bg-danger'}"></span>${connected ? t('connection.connected') : t('connection.offline')}`;
  els.navStatusDot.className = `h-2.5 w-2.5 rounded-full ${connected ? 'bg-primary' : 'bg-danger'}`;
  els.navStatusText.textContent = connected ? t('connection.connected') : t('connection.offline');
  const serviceActions = document.getElementById('serviceActions');
  if (serviceActions) {
    serviceActions.innerHTML = connected ? '' : `
      <button data-service-action="start" class="inline-flex items-center gap-2 rounded-md bg-primary px-3 py-2 text-sm font-bold text-slate-950">
        <span class="material-symbols-outlined text-[18px]">play_arrow</span>${t('connection.startService')}
      </button>
      <button data-service-action="restart" class="inline-flex items-center gap-2 rounded-md border border-danger px-3 py-2 text-sm font-semibold text-danger">
        <span class="material-symbols-outlined text-[18px]">restart_alt</span>${t('connection.restartService')}
      </button>
    `;
  }
}

function sectionHeader(title, description, actions) {
  return `
    <div class="flex flex-wrap items-end justify-between gap-4">
      <div>
        <h1 class="font-display text-3xl font-semibold">${title}</h1>
        <p class="mt-2 max-w-3xl text-sm text-muted">${description}</p>
      </div>
      <div class="flex flex-wrap gap-2">
        ${actions.map(([icon, label, action]) => `<button data-header-action="${action}" class="inline-flex items-center gap-2 rounded-md border border-outline bg-surface px-3 py-2 text-sm font-semibold hover:bg-surface-high"><span class="material-symbols-outlined text-[18px]">${icon}</span>${label}</button>`).join('')}
      </div>
    </div>
  `;
}

function metricCard(label, text, hint, severity) {
  return `<article class="rounded-lg border border-outline bg-surface p-5 ${severity === 'Critical' ? 'glow-danger' : ''}"><p class="text-sm font-semibold text-muted">${label}</p><div class="mt-3 flex items-center justify-between gap-3"><div class="font-display text-2xl font-semibold">${escapeHtml(text)}</div>${severityIcon(severity)}</div><p class="mt-3 text-sm text-muted">${escapeHtml(hint)}</p></article>`;
}

function settingsCard(title, icon, content, cols) {
  return `<section class="${cols} rounded-lg border border-outline bg-surface p-5"><div class="mb-4 flex items-center gap-2 border-b border-outline pb-3"><span class="material-symbols-outlined text-primary">${icon}</span><h2 class="font-display text-lg font-semibold">${title}</h2></div><div class="space-y-4">${content}</div></section>`;
}

function backupScheduleFields(schedule) {
  const mode = schedule.backupScheduleMode || schedule.BackupScheduleMode || 'weekly';
  const time = schedule.backupScheduleTime || schedule.BackupScheduleTime || '04:00';
  const weekDays = schedule.backupScheduleWeekDays || schedule.BackupScheduleWeekDays || [1];
  const monthDays = schedule.backupScheduleMonthDays || schedule.BackupScheduleMonthDays || [1];
  const weekOrder = [1, 2, 3, 4, 5, 6, 0];
  return `
    <div class="rounded-lg border border-outline bg-background p-3">
      <div class="text-sm font-semibold text-muted">${t('settings.backupScheduleMode')}</div>
      <div class="mt-3 grid gap-2 sm:grid-cols-2">
        ${radioPill('backupScheduleMode', 'weekly', t('settings.weekly'), mode === 'weekly')}
        ${radioPill('backupScheduleMode', 'monthly', t('settings.monthly'), mode === 'monthly')}
      </div>
    </div>
    <label class="block">
      <span class="text-sm font-semibold text-muted">${t('settings.backupTime')}</span>
      <input name="backupScheduleTime" type="time" value="${escapeAttr(time)}" class="mt-2 w-full rounded-md border-outline bg-background font-mono text-sm">
    </label>
    <div data-backup-schedule-section="weekly" class="rounded-lg border border-outline bg-background p-3">
      <div class="mb-3 text-sm font-semibold text-muted">${t('settings.weekdays')}</div>
      <div class="grid grid-cols-3 gap-2 sm:grid-cols-7">
        ${weekOrder.map((day) => checkboxPill('backupScheduleWeekDays', day, t('settings.weekdaysShort')[day], weekDays.map(Number).includes(day))).join('')}
      </div>
    </div>
    <div data-backup-schedule-section="monthly" class="rounded-lg border border-outline bg-background p-3">
      <div class="mb-3 text-sm font-semibold text-muted">${t('settings.monthDays')}</div>
      <div class="grid grid-cols-7 gap-2 sm:grid-cols-10">
        ${Array.from({ length: 31 }, (_, i) => i + 1).map((day) => checkboxPill('backupScheduleMonthDays', day, String(day), monthDays.map(Number).includes(day))).join('')}
      </div>
    </div>
  `;
}

function radioPill(name, valueText, label, checked) {
  return `<label class="flex cursor-pointer items-center justify-center gap-2 rounded-md border border-outline px-3 py-2 text-sm font-semibold has-[:checked]:border-primary has-[:checked]:bg-primary/10 has-[:checked]:text-primary"><input name="${name}" value="${valueText}" type="radio" class="sr-only" ${checked ? 'checked' : ''}>${label}</label>`;
}

function checkboxPill(name, valueText, label, checked) {
  return `<label class="flex cursor-pointer items-center justify-center rounded-md border border-outline px-2 py-2 text-sm font-semibold has-[:checked]:border-primary has-[:checked]:bg-primary/10 has-[:checked]:text-primary"><input name="${name}" value="${valueText}" type="checkbox" class="sr-only" ${checked ? 'checked' : ''}>${label}</label>`;
}

function updateBackupScheduleVisibility() {
  const form = document.getElementById('settingsForm');
  if (!form) return;
  const mode = new FormData(form).get('backupScheduleMode') || 'weekly';
  document.querySelectorAll('[data-backup-schedule-section]').forEach((section) => {
    section.classList.toggle('hidden', section.dataset.backupScheduleSection !== mode);
  });
}

function actionCard(icon, title, description, url) {
  return `<article class="rounded-lg border border-outline bg-surface p-5"><span class="material-symbols-outlined text-4xl text-primary">${icon}</span><h2 class="mt-3 font-display text-xl font-semibold">${title}</h2><p class="mt-2 text-sm text-muted">${description}</p><button data-run-url="${url}" class="mt-5 rounded-md bg-primary px-3 py-2 text-sm font-bold text-slate-950">${t('common.run')}</button></article>`;
}

function diagnosticCard(label, text, severity) {
  return `<article class="rounded-lg border border-outline bg-surface p-5"><p class="text-sm text-muted">${label}</p><div class="mt-3">${looksLikePath(text) ? pathChip(text) : `<p class="break-words font-mono text-sm">${escapeHtml(text)}</p>`}</div><div class="mt-4">${stateBadge(severity)}</div></article>`;
}

function emptyState(icon, title, text) {
  return `<div class="p-6 text-center text-muted"><span class="material-symbols-outlined text-4xl">${icon}</span><div class="mt-2 font-semibold text-content">${title}</div><div class="mt-1 text-sm">${text}</div></div>`;
}

function issueCard(issue) {
  return `<article class="rounded-lg border-l-4 ${issue.severity === 'Critical' ? 'border-danger bg-danger/10 text-danger' : 'border-warning bg-warning/10 text-warning'} p-4"><div class="flex items-center gap-2 font-semibold"><span class="material-symbols-outlined">${issue.severity === 'Critical' ? 'report_problem' : 'warning'}</span>${issue.title}</div><p class="mt-2 text-sm">${issue.description}</p></article>`;
}

function toggleField(label, name, checked) {
  return `<label class="flex items-center justify-between gap-4 rounded-lg border border-outline bg-background p-3"><span class="font-semibold">${label}</span><input name="${name}" type="checkbox" class="rounded border-outline text-primary" ${checked ? 'checked' : ''}></label>`;
}

function textField(label, name, valueText) {
  return `<label class="block"><span class="text-sm font-semibold text-muted">${label}</span><input name="${name}" value="${escapeAttr(valueText ?? '')}" class="mt-2 w-full rounded-md border-outline bg-background font-mono text-sm"></label>`;
}

function textInput(label, name, valueText, multiline = false, browse = false) {
  const common = `name="${name}" data-path-input="${browse ? 'true' : 'false'}" data-multiline="${multiline ? 'true' : 'false'}" class="w-full rounded-md border-outline bg-background font-mono text-sm" autocomplete="off"`;
  const control = multiline ? `<textarea ${common} rows="4">${escapeHtml(valueText || '')}</textarea>` : `<input ${common} value="${escapeAttr(valueText || '')}">`;
  return `
    <div class="block" data-path-input-wrap>
      <span class="text-sm font-semibold text-muted">${label}</span>
      <div class="mt-2 flex gap-2">
        ${control}
        ${browse ? `<button type="button" data-browse-path class="shrink-0 rounded-md border border-outline px-3 py-2 text-sm font-semibold hover:bg-surface-high">${t('common.browse')}</button>` : ''}
      </div>
      ${browse ? '<div data-path-suggestions class="mt-2 hidden flex-wrap gap-2"></div>' : ''}
    </div>
  `;
}

function checkInput(label, name, checked) {
  return `<label class="flex items-center gap-3"><input name="${name}" type="checkbox" class="rounded border-outline text-primary" ${checked ? 'checked' : ''}><span>${label}</span></label>`;
}

function modalButtons() {
  return `<div class="flex justify-end gap-2"><button type="button" data-close-edit class="rounded-md border border-outline px-3 py-2 text-sm">${t('common.cancel')}</button><button class="rounded-md bg-primary px-3 py-2 text-sm font-bold text-slate-950">${t('common.save')}</button></div>`;
}

function pathLine(label, path) {
  return `<div><div class="text-xs font-semibold text-muted">${label}</div><div class="mt-1">${pathChip(path || '-')}</div></div>`;
}

function pathList(title, paths, color = '') {
  return `<div><h3 class="mb-2 font-semibold ${color}">${title}</h3><div class="space-y-2">${paths.length ? paths.map(pathChip).join('') : pathChip('-')}</div></div>`;
}

function pathChip(path) {
  return `<span class="inline-flex max-w-full items-center rounded-md border border-outline bg-background px-2 py-1 font-mono text-xs text-muted"><span class="truncate">${escapeHtml(path || '-')}</span></span>`;
}

function smallStat(label, text) {
  return `<div class="rounded-lg border border-outline bg-background p-3"><div class="text-xs text-muted">${label}</div><div class="mt-1 font-semibold">${escapeHtml(text || '-')}</div></div>`;
}

function togglePill(enabled) {
  return enabled ? `<span class="rounded-md border border-primary bg-primary/10 px-2 py-1 text-xs font-bold text-primary">${t('common.enabled')}</span>` : `<span class="rounded-md border border-outline bg-background px-2 py-1 text-xs font-bold text-muted">${t('common.disabled')}</span>`;
}

function stateBadge(status) {
  return `<span class="inline-flex items-center rounded-md border px-2 py-1 text-xs font-bold ${statusClass(status)}">${escapeHtml(translateState(status))}</span>`;
}

function severityIcon(severity) {
  const icon = severity === 'Critical' ? 'error' : severity === 'Warning' ? 'warning' : severity === 'Busy' ? 'sync' : 'shield';
  const color = severity === 'Critical' ? 'text-danger' : severity === 'Warning' ? 'text-warning' : severity === 'Busy' ? 'text-secondary' : 'text-primary';
  return `<span class="material-symbols-outlined ${color}">${icon}</span>`;
}

function statusClass(status) {
  const s = String(status || '').toLowerCase();
  if (['ok', 'success'].includes(s)) return 'border-primary bg-primary/10 text-primary';
  if (['busy', 'running'].includes(s)) return 'border-secondary bg-secondary/10 text-secondary';
  if (['warning', 'partiallydone', 'retrywaiting'].includes(s)) return 'border-warning bg-warning/10 text-warning';
  if (['critical', 'failed'].includes(s)) return 'border-danger bg-danger/10 text-danger';
  return 'border-outline bg-background text-muted';
}

function collectIssues(syncStatuses, backupStatuses, severity) {
  const issues = [];
  if (severity === 'Critical') issues.push({ severity: 'Critical', title: translateSeverity(severity), description: value(state.status || {}, 'trayTooltip', 'TrayTooltip') || translateSeverity(severity) });
  for (const row of [...syncStatuses, ...backupStatuses]) {
    const rowState = value(row, 'lastState', 'LastState');
    const error = value(row, 'lastErrorMessage', 'LastErrorMessage');
    if (rowState === 'Failed') issues.push({ severity: 'Critical', title: translateState(rowState), description: error || value(row, 'sourcePath', 'SourcePath') });
    if (rowState === 'PartiallyDone') issues.push({ severity: 'Warning', title: translateState(rowState), description: error || value(row, 'sourcePath', 'SourcePath') });
  }
  return issues;
}

function infoBlock(label, text, path = false) {
  return `<div class="rounded-lg border border-outline bg-background p-3"><div class="text-xs text-muted">${label}</div><div class="mt-1 truncate font-mono text-sm">${path ? pathChip(text) : escapeHtml(text)}</div></div>`;
}

function destinationMetaForSync(pair, path) {
  const locations = pair.targetLocations || pair.TargetLocations || {};
  return locations[path] || Object.entries(locations).find(([key]) => key.toLowerCase() === String(path).toLowerCase())?.[1] || null;
}

function pairHasSyncError(pair) {
  const source = String(pair.sourcePath || pair.SourcePath || '');
  const targets = pair.targetPaths || pair.TargetPaths || [];
  const statusRows = value(state.status || {}, 'syncStatuses', 'SyncStatuses') || [];
  const hasFailedRun = statusRows.some((row) => {
    const rowSource = String(value(row, 'sourcePath', 'SourcePath') || '');
    const rowTarget = String(value(row, 'targetPath', 'TargetPath') || '');
    const rowState = String(value(row, 'lastState', 'LastState') || '').toLowerCase();
    return rowSource.toLowerCase() === source.toLowerCase()
      && targets.some((target) => String(target).toLowerCase() === rowTarget.toLowerCase())
      && ['failed', 'partiallydone', 'retrywaiting'].includes(rowState);
  });
  if (hasFailedRun) return true;
  const sourceProbe = sourceMetaForSync(pair)?.lastProbe || sourceMetaForSync(pair)?.LastProbe;
  if (sourceProbe && !(value(sourceProbe, 'available', 'Available') && value(sourceProbe, 'writable', 'Writable'))) return true;
  return targets.some((target) => {
    const probe = destinationMetaForSync(pair, target)?.lastProbe || destinationMetaForSync(pair, target)?.LastProbe;
    return probe && !(value(probe, 'available', 'Available') && value(probe, 'writable', 'Writable'));
  });
}

function sourceMetaForSync(pair) {
  return pair.sourceLocation || pair.SourceLocation || null;
}

function syncDestinationStatusLine(pair, targetPath) {
  return `
    <div class="space-y-2 rounded-md border border-outline bg-background p-2">
      ${destinationStatusLine(pair.sourcePath, sourceMetaForSync(pair), { role: t('destination.source'), sourcePath: pair.sourcePath, targetPath })}
      ${destinationStatusLine(targetPath, destinationMetaForSync(pair, targetPath), { role: t('destination.target'), sourcePath: pair.sourcePath, targetPath })}
    </div>
  `;
}

function destinationStatusLine(location, endpoint, options = {}) {
  const probe = endpoint?.lastProbe || endpoint?.LastProbe || null;
  const identity = endpoint?.identity || endpoint?.Identity || probe?.identity || probe?.Identity || null;
  const kind = endpoint?.kind ?? endpoint?.Kind ?? probe?.kind ?? probe?.Kind ?? 'Unknown';
  const ok = probe ? Boolean(value(probe, 'available', 'Available') && value(probe, 'writable', 'Writable')) : null;
  const text = ok === null ? t('destination.unknown') : ok ? `${t('destination.available')} (${t('destination.writable')})` : t('destination.unavailable');
  const error = value(probe, 'diagnosticDetails', 'DiagnosticDetails') || value(probe, 'errorMessage', 'ErrorMessage') || '';
  const fingerprint = value(identity, 'fingerprint', 'Fingerprint');
  const title = [error, fingerprint ? `ID: ${fingerprint}` : null].filter(Boolean).join('\n');
  const cls = ok === null ? 'border-outline text-muted' : ok ? 'border-primary text-primary' : 'border-warning text-warning';
  const button = options.sourcePath && options.targetPath
    ? `<button data-retest-sync-source="${escapeAttr(options.sourcePath)}" data-retest-sync-target="${escapeAttr(options.targetPath)}" class="rounded-md border border-outline px-2 py-1 text-xs">${t('common.retest')}</button>`
    : `<button data-retest-destination="${escapeAttr(location)}" class="rounded-md border border-outline px-2 py-1 text-xs">${t('common.retest')}</button>`;
  return `
    <div class="flex flex-wrap items-center gap-2 rounded-md border border-outline bg-background p-2 text-xs">
      ${options.role ? `<span class="font-semibold text-muted">${escapeHtml(options.role)}:</span>` : ''}
      <span class="max-w-full truncate font-mono text-muted">${escapeHtml(location)}</span>
      <span title="${escapeAttr(title)}" class="inline-flex items-center gap-1 rounded-md border px-2 py-1 font-semibold ${cls}">
        <span class="material-symbols-outlined text-[16px]">${ok ? 'check_circle' : ok === false ? 'warning' : 'help'}</span>
        ${text}
      </span>
      <span class="text-muted">${t('destination.kind')}: ${escapeHtml(String(kind))}</span>
      <span class="font-mono text-muted">${escapeHtml(formatDate(value(probe, 'testedAt', 'TestedAt')))}</span>
      ${button}
    </div>
  `;
}

function modeOption(valueNumber, label, current) {
  return `<option value="${valueNumber}" ${Number(current ?? 1) === valueNumber ? 'selected' : ''}>${label}</option>`;
}

function option(valueNumber, label, current, defaultValue = 0) {
  return `<option value="${valueNumber}" ${Number(current ?? defaultValue) === valueNumber ? 'selected' : ''}>${label}</option>`;
}

function isBackupNameUnique(name, currentIndex = null) {
  const normalized = name.trim().toLowerCase();
  return (state.config?.backupSources || []).every((backup, index) => {
    if (currentIndex !== null && index === currentIndex) return true;
    return String(backup.name || backup.Name || '').trim().toLowerCase() !== normalized;
  });
}

function panelEl(id) { return document.querySelector(`[data-panel="${id}"]`); }
function value(obj, camel, pascal) { return obj?.[camel] ?? obj?.[pascal]; }
function t(path) {
  return path.split('.').reduce((current, key) => current?.[key], dictionaries[state.language]) ?? path;
}
function translateSeverity(input) { return t(`states.${input}`) || input || '-'; }
function translateState(input) { return t(`states.${input}`) || input || '-'; }
function translateTaskType(input) { return t(`states.${input}`) || input || '-'; }
function translateMode(input) { return t('modes')[Number(input ?? 1)] || t('modes')[1]; }
function newestDate(rows, camel, pascal) {
  const dates = rows.map(row => value(row, camel, pascal)).filter(Boolean).sort();
  return dates.length ? formatDate(dates.at(-1)) : '-';
}
function formatDate(input) {
  if (!input) return '-';
  return new Date(input).toLocaleString(state.language === 'pl' ? 'pl-PL' : 'en-US', { dateStyle: 'short', timeStyle: 'short' });
}
function formatDuration(input) {
  if (!input) return '-';
  const [h, m] = String(input).split(':').map(Number);
  if (h >= 24) return `${Math.round(h / 24)} d`;
  if (h > 0) return `${h}h`;
  return `${m} min`;
}
function backupScheduleSummary(schedule = {}) {
  const mode = schedule.backupScheduleMode || schedule.BackupScheduleMode || 'weekly';
  const time = schedule.backupScheduleTime || schedule.BackupScheduleTime || '04:00';
  if (mode === 'monthly') {
    const days = schedule.backupScheduleMonthDays || schedule.BackupScheduleMonthDays || [1];
    return `${t('settings.monthly')}: ${days.join(', ')} ${time}`;
  }
  const days = schedule.backupScheduleWeekDays || schedule.BackupScheduleWeekDays || [1];
  const names = t('settings.weekdaysShort');
  return `${t('settings.weekly')}: ${days.map((day) => names[Number(day)] || day).join(', ')} ${time}`;
}
function durationToHours(input) {
  const [h] = String(input || '02:00:00').split(':').map(Number);
  return Number.isFinite(h) ? h : 2;
}
function bytesToGb(input) { return Math.round(Number(input || 0) / 1024 / 1024 / 1024) || 300; }
function formatBytes(bytes) {
  const units = ['B', 'KB', 'MB', 'GB', 'TB'];
  let n = Number(bytes || 0);
  let i = 0;
  while (n >= 1024 && i < units.length - 1) { n /= 1024; i++; }
  return `${n.toFixed(i === 0 || n >= 10 ? 0 : 1)} ${units[i]}`;
}
function formatLogLine(line) {
  return `[${new Date(value(line, 'at', 'At')).toLocaleTimeString(state.language === 'pl' ? 'pl-PL' : 'en-US')}] [${value(line, 'level', 'Level')}] ${value(line, 'message', 'Message')}`;
}
function looksLikePath(text) { return typeof text === 'string' && (text.includes('\\') || text.includes('/')); }
function getPreference(name) {
  const cookie = Object.fromEntries(document.cookie.split(';').map(x => x.trim()).filter(Boolean).map(x => {
    const [k, ...v] = x.split('=');
    return [decodeURIComponent(k), decodeURIComponent(v.join('='))];
  }));
  return cookie[name] || localStorage.getItem(name);
}
function setPreference(name, valueText) {
  localStorage.setItem(name, valueText);
  document.cookie = `${encodeURIComponent(name)}=${encodeURIComponent(valueText)};path=/;max-age=31536000;samesite=lax`;
}
function cloneJson(input) { return JSON.parse(JSON.stringify(input)); }
function escapeHtml(input) {
  return String(input ?? '').replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;').replaceAll('"', '&quot;').replaceAll("'", '&#039;');
}
function escapeAttr(input) { return escapeHtml(input).replaceAll('\n', '&#10;'); }
