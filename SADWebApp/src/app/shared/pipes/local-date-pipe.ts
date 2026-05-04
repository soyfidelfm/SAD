import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'localDate',
  standalone: true
})
export class LocalDatePipe implements PipeTransform {

  transform(value: string | Date | null | undefined): string {
    if (!value) return '';

    const userTimeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;

    return new Date(value).toLocaleString('en-US', {
      timeZone: userTimeZone,
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}