export function capitalizeWords(input) {
  if (typeof input === 'number') {
    input = String(input); // Convert numbers to strings
  }

  if (typeof input !== 'string') return '';
  if (input.length === 0) return '';

  return input.replace(/\b\w|_/g, function(match) {
    if (match === '_') {
      return ' '; // Replace underscores with whitespace
    } else {
      return match.toUpperCase();
    }
  });
}
