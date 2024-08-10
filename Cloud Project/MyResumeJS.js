document.addEventListener('DOMContentLoaded', () => {
    const footer = document.getElementById('footer');
    const date = new Date();
    footer.innerHTML += `<p>Last updated: ${date.toDateString()}</p>`;
  });
  