// Funciones auxiliares para la tabla de contenidos
(function() {
  'use strict';

  // Función para obtener el nivel de un heading
  function getHeadingLevel(tagName) {
    return parseInt(tagName.charAt(1));
  }

  // Función para crear ID único para heading
  function createHeadingId(text, existingIds) {
    let id = text.toLowerCase()
      .replace(/[^\w\s-]/g, '')
      .replace(/\s+/g, '-')
      .replace(/-+/g, '-')
      .trim();
    
    // Asegurar unicidad
    let uniqueId = id;
    let counter = 1;
    while (existingIds.includes(uniqueId)) {
      uniqueId = id + '-' + counter;
      counter++;
    }
    
    return uniqueId;
  }

  // Exportar funciones si es necesario
  if (typeof window !== 'undefined') {
    window.TOCUtils = {
      getHeadingLevel: getHeadingLevel,
      createHeadingId: createHeadingId
    };
  }
})();

