import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'localDate',
  standalone: true
})
export class LocalDatePipe implements PipeTransform {

  transform(
    value: string | Date | null | undefined,
    format: 'datetime' | 'pretty' | 'date' = 'datetime'
  ): string {

    if (!value) {
      return '';
    }

    const date = new Date(value);

    const userTimeZone =
      Intl.DateTimeFormat().resolvedOptions().timeZone;

    switch (format) {

      case 'pretty':
        return this.prettyDate(date);

      case 'date':
        return date.toLocaleDateString('en-US', {
          timeZone: userTimeZone,
          year: 'numeric',
          month: 'short',
          day: 'numeric'
        });

      default:
        return date.toLocaleString('en-US', {
          timeZone: userTimeZone,
          year: 'numeric',
          month: 'short',
          day: 'numeric',
          hour: '2-digit',
          minute: '2-digit'
        });
    }
  }

  private prettyDate(date: Date): string {

    const month = date.toLocaleString('en-US', {
      month: 'long'
    });

    const day = date.getDate();

    const year = date.getFullYear();

    return `${month} ${day}${this.getSuffix(day)} ${year}`;
  }

  private getSuffix(day: number): string {

    if (day >= 11 && day <= 13) {
      return 'th';
    }

    switch (day % 10) {

      case 1:
        return 'st';

      case 2:
        return 'nd';

      case 3:
        return 'rd';

      default:
        return 'th';
    }
  }
}