(function() {
  'use strict';

  // Cargar el contenido markdown
  async function loadDocumentation() {
    try {
      // Usar PathBase si está disponible (inyectado desde el servidor)
      const pathBase = window.APP_PATH_BASE || '';
      const markdownUrl = `${pathBase}/docs/index.md`;
      const response = await fetch(markdownUrl);
      const markdown = await response.text();
      
      // Convertir markdown a HTML usando marked
      if (typeof marked !== 'undefined') {
        // Función para escapar HTML
        function escapeHtml(text) {
          const map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
          };
          return text.replace(/[&<>"']/g, function(m) { return map[m]; });
        }
        
        // Configurar renderer personalizado para bloques de código
        const renderer = new marked.Renderer();
        renderer.code = function(code, language) {
          const lang = language || 'text';
          return `<pre class="language-${lang}"><code class="language-${lang}">${escapeHtml(code)}</code></pre>`;
        };
        
        const html = marked.parse(markdown, {
          breaks: true,
          gfm: true,
          renderer: renderer
        });
        
        document.getElementById('documentation').innerHTML = html;
        
        // Generar tabla de contenidos
        generateTOC();
        
        // Aplicar syntax highlighting después de un breve delay para que Prism cargue los lenguajes
        setTimeout(function() {
          if (typeof Prism !== 'undefined') {
            Prism.highlightAll();
          }
        }, 100);
        
        // Manejar selección de lenguaje de código
        setupLanguageSelector();
      } else {
        console.error('marked.js no está cargado');
      }
    } catch (error) {
      console.error('Error al cargar la documentación:', error);
      document.getElementById('documentation').innerHTML = 
        '<h1>Error</h1><p>No se pudo cargar la documentación. Por favor, intenta más tarde.</p>';
    }
  }

  // Generar tabla de contenidos
  function generateTOC() {
    const toc = document.getElementById('toc');
    const headings = document.querySelectorAll('#documentation h1, #documentation h2, #documentation h3');
    
    if (!toc || headings.length === 0) return;
    
    const tocList = document.createElement('ul');
    tocList.className = 'tocify';
    
    headings.forEach((heading, index) => {
      const level = parseInt(heading.tagName.charAt(1));
      const id = heading.textContent.toLowerCase()
        .replace(/[^\w\s-]/g, '')
        .replace(/\s+/g, '-')
        .replace(/-+/g, '-');
      
      heading.id = id || `heading-${index}`;
      
      const li = document.createElement('li');
      li.className = `tocify-item level-${level}`;
      
      const a = document.createElement('a');
      a.href = '#' + heading.id;
      a.textContent = heading.textContent;
      a.onclick = function(e) {
        e.preventDefault();
        document.getElementById(heading.id).scrollIntoView({ behavior: 'smooth' });
        return false;
      };
      
      li.appendChild(a);
      tocList.appendChild(li);
    });
    
    toc.appendChild(tocList);
    
    // Manejar scroll para resaltar item activo
    handleScrollHighlight();
  }

  // Resaltar item activo en TOC al hacer scroll
  function handleScrollHighlight() {
    const items = document.querySelectorAll('.tocify-item > a');
    const headings = document.querySelectorAll('#documentation h1, #documentation h2, #documentation h3');
    
    function updateActiveItem() {
      let current = '';
      
      headings.forEach((heading) => {
        const rect = heading.getBoundingClientRect();
        if (rect.top <= 100) {
          current = heading.id;
        }
      });
      
      items.forEach((item) => {
        const parent = item.parentElement;
        if (item.getAttribute('href') === '#' + current) {
          parent.classList.add('active');
        } else {
          parent.classList.remove('active');
        }
      });
    }
    
    window.addEventListener('scroll', updateActiveItem);
    updateActiveItem();
  }

  // Configurar selector de lenguaje
  function setupLanguageSelector() {
    const langLinks = document.querySelectorAll('.lang-selector a');
    const codeBlocks = document.querySelectorAll('pre code');
    
    langLinks.forEach((link) => {
      link.addEventListener('click', function(e) {
        e.preventDefault();
        
        // Remover activo de todos
        langLinks.forEach(l => l.classList.remove('active'));
        
        // Agregar activo al seleccionado
        this.classList.add('active');
        
        const langName = this.getAttribute('data-language-name');
        // Aquí podrías implementar cambio de ejemplos de código si fuera necesario
      });
    });
    
    // Activar primer lenguaje por defecto
    if (langLinks.length > 0) {
      langLinks[0].classList.add('active');
    }
  }

  // Búsqueda simple
  function setupSearch() {
    const searchInput = document.getElementById('input-search');
    const searchResults = document.querySelector('.search-results');
    
    if (!searchInput || !searchResults) return;
    
    searchInput.addEventListener('input', function() {
      const query = this.value.toLowerCase().trim();
      
      if (query.length < 2) {
        searchResults.innerHTML = '';
        searchResults.style.display = 'none';
        return;
      }
      
      const headings = document.querySelectorAll('#documentation h1, #documentation h2, #documentation h3, #documentation h4');
      const results = [];
      
      headings.forEach((heading) => {
        const text = heading.textContent.toLowerCase();
        if (text.includes(query)) {
          results.push({
            text: heading.textContent,
            id: heading.id,
            level: heading.tagName
          });
        }
      });
      
      if (results.length > 0) {
        searchResults.innerHTML = results.map(r => 
          `<li><a href="#${r.id}">${r.text}</a></li>`
        ).join('');
        searchResults.style.display = 'block';
      } else {
        searchResults.innerHTML = '<li><a href="#">No se encontraron resultados</a></li>';
        searchResults.style.display = 'block';
      }
    });
    
    // Manejar clic en resultados de búsqueda
    searchResults.addEventListener('click', function(e) {
      if (e.target.tagName === 'A') {
        e.preventDefault();
        const href = e.target.getAttribute('href');
        if (href && href !== '#') {
          document.querySelector(href).scrollIntoView({ behavior: 'smooth' });
          searchResults.style.display = 'none';
          searchInput.value = '';
        }
      }
    });
  }

  // Toggle navegación móvil
  function setupMobileNav() {
    const navButton = document.getElementById('nav-button');
    const tocWrapper = document.querySelector('.tocify-wrapper');
    
    if (navButton && tocWrapper) {
      navButton.addEventListener('click', function(e) {
        e.preventDefault();
        tocWrapper.style.width = tocWrapper.style.width === '230px' ? '0' : '230px';
      });
    }
  }

  // Inicializar cuando el DOM esté listo
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function() {
      loadDocumentation();
      setupSearch();
      setupMobileNav();
    });
  } else {
    loadDocumentation();
    setupSearch();
    setupMobileNav();
  }
})();

