import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'rawDate',
  standalone: true
})
export class RawDatePipe implements PipeTransform {

  transform(
    value: string | Date | null | undefined,
    format: 'datetime' | 'pretty' | 'date' = 'datetime'
  ): string {

    if (!value) {
      return '';
    }

    const date =
      typeof value === 'string'
        ? this.parseWithoutTimezone(value)
        : value;

    switch (format) {

      case 'pretty':
        return this.prettyDate(date);

      case 'date':
        return date.toLocaleDateString('en-US', {
          year: 'numeric',
          month: 'short',
          day: 'numeric'
        });

      default:
        return date.toLocaleString('en-US', {
          year: 'numeric',
          month: 'short',
          day: 'numeric',
          hour: '2-digit',
          minute: '2-digit',
          second: '2-digit'
        });
    }
  }

  private parseWithoutTimezone(value: string): Date {

    // Removes timezone conversion completely
    // Keeps the exact numbers from backend

    const cleaned = value
      .replace('Z', '')
      .split('.')[0];

    const [datePart, timePart] = cleaned.split('T');

    const [year, month, day] =
      datePart.split('-').map(Number);

    const [hour = 0, minute = 0, second = 0] =
      (timePart || '')
        .split(':')
        .map(Number);

    return new Date(
      year,
      month - 1,
      day,
      hour,
      minute,
      second
    );
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