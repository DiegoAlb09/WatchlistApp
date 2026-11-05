// Es "claro primero": quien nunca eligio tema ve el claro.
// Quien ya habia elegido oscuro (guardado en localStorage) lo conserva.
window.themeInterop = {
  get: function () {
    return localStorage.getItem('theme') || 'light';
  },
  set: function (theme) {
    localStorage.setItem('theme', theme);
    document.documentElement.setAttribute('data-theme', theme);
  },
  apply: function () {
    const theme = localStorage.getItem('theme') || 'light';
    document.documentElement.setAttribute('data-theme', theme);
  }
};