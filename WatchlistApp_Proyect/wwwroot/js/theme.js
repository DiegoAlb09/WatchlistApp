window.themeInterop = {
  get: function () {
    return localStorage.getItem('theme') || 'dark';
  },
  set: function (theme) {
    localStorage.setItem('theme', theme);
    document.documentElement.setAttribute('data-theme', theme);
  },
  apply: function () {
    const theme = localStorage.getItem('theme') || 'dark';
    document.documentElement.setAttribute('data-theme', theme);
  }
}