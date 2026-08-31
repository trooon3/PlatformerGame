mergeInto(LibraryManager.library, {
    InitVisibilityChangeEvent: function() {
        document.addEventListener('visibilitychange', function() {
            if (document.hidden) {
                // Если мы ушли с вкладки, отправляем сигнал объекту AudioController
                SendMessage('AudioController', 'MuteAudioBackground');
            } else {
                // Вернулись на вкладку
                SendMessage('AudioController', 'UnmuteAudioBackground');
            }
        });
    }
});