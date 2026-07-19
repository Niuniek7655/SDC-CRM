import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-forbidden',
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="forbidden">
      <h1>Brak dostępu</h1>
      <p>Twoje konto nie ma uprawnień do tej sekcji. Skontaktuj się z administratorem.</p>
      <a routerLink="/">Wróć na stronę główną</a>
    </section>
  `,
  styles: [
    `
      .forbidden {
        max-width: 32rem;
        margin: 3rem auto;
        text-align: center;
      }
    `,
  ],
})
export class Forbidden {}
