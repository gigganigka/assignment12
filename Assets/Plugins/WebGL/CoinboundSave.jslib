mergeInto(LibraryManager.library, {
  CoinboundFlushSave: function () {
    if (Module.coinboundSyncBusy) { Module.coinboundSyncAgain = true; return; }
    var sync = function () {
      Module.coinboundSyncBusy = true;
      FS.syncfs(false, function (error) {
        Module.coinboundSyncBusy = false;
        if (error) console.warn('Coinbound: browser save sync failed', error);
        if (Module.coinboundSyncAgain) { Module.coinboundSyncAgain = false; sync(); }
      });
    };
    sync();
  }
});
