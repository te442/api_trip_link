import {
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  OnDestroy,
  OnInit,
  Output,
  ViewChild
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, Subscription, of } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { PlaceSuggestion, PlacesService } from '../../services/places.service';

@Component({
  selector: 'app-address-autocomplete',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="address-field">
      <div class="input-wrap">
        <input
          #addressInput
          type="text"
          class="address-input"
          [class.invalid]="showError"
          [value]="value"
          [placeholder]="placeholder"
          [disabled]="disabled"
          (input)="onInput($event)"
          (focus)="onFocus()"
          (keydown)="onKeydown($event)"
          autocomplete="off"
          autocorrect="off"
          autocapitalize="off"
          spellcheck="false"
          name="trip-start-address"
          role="combobox"
          aria-autocomplete="list"
          [attr.aria-expanded]="suggestionsOpen"
        />
        <span *ngIf="validated" class="valid-badge" title="כתובת אומתה">✓</span>
      </div>

      <ul *ngIf="suggestionsOpen && suggestions.length" class="suggestions" role="listbox">
        <li *ngFor="let s of suggestions; let i = index"
            role="option"
            [class.active]="i === activeIndex"
            (mousedown)="selectSuggestion(s, $event)">
          <span class="main">{{ s.mainText || s.description }}</span>
          <span *ngIf="s.secondaryText" class="secondary">{{ s.secondaryText }}</span>
        </li>
      </ul>

      <p *ngIf="searching" class="hint">מחפש כתובות...</p>
      <p *ngIf="apiError" class="hint error">{{ apiError }}</p>
      <p *ngIf="showError" class="hint error">יש לבחור כתובת מהרשימה</p>
      <p *ngIf="!searching && !apiError && !validated && value.trim().length >= 2 && !suggestions.length && lastSearched"
         class="hint warn">לא נמצאו כתובות — נסי ניסוח אחר</p>
      <p *ngIf="validated" class="hint ok">כתובת נבחרה בהצלחה</p>
    </div>
  `,
  styles: []
})
export class AddressAutocompleteComponent implements OnInit, OnDestroy {
  @ViewChild('addressInput', { static: true }) addressInput!: ElementRef<HTMLInputElement>;

  @Input() value = '';
  @Input() placeholder = 'התחילי להקליד כתובת...';
  @Input() disabled = false;
  @Input() validated = false;
  @Input() showErrorWhenInvalid = false;

  @Output() valueChange = new EventEmitter<string>();
  @Output() validatedChange = new EventEmitter<boolean>();

  suggestions: PlaceSuggestion[] = [];
  suggestionsOpen = false;
  searching = false;
  lastSearched = false;
  activeIndex = -1;
  apiError = '';

  private readonly search$ = new Subject<string>();
  private sub?: Subscription;

  constructor(
    private places: PlacesService,
    private el: ElementRef<HTMLElement>
  ) {}

  ngOnInit(): void {
    this.sub = this.search$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(q => {
        this.searching = true;
        this.apiError = '';
        this.lastSearched = false;
        return this.places.autocomplete(q).pipe(
          catchError(err => {
            this.apiError = err?.message || 'שגיאה בחיפוש כתובות';
            return of<PlaceSuggestion[]>([]);
          })
        );
      })
    ).subscribe({
      next: list => {
        this.suggestions = list;
        this.searching = false;
        this.lastSearched = true;
        this.suggestionsOpen = list.length > 0;
        this.activeIndex = list.length ? 0 : -1;
      },
      error: err => {
        this.searching = false;
        this.lastSearched = true;
        this.suggestions = [];
        this.suggestionsOpen = false;
        this.apiError = err?.message || 'שגיאה בחיפוש כתובות';
      }
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  get showError(): boolean {
    return this.showErrorWhenInvalid && !this.validated && !!this.value.trim();
  }

  onFocus(): void {
    if (this.suggestions.length) this.suggestionsOpen = true;
  }

  onInput(event: Event): void {
    const text = (event.target as HTMLInputElement).value;
    this.value = text;
    this.valueChange.emit(text);
    this.setValidated(false);
    this.suggestionsOpen = false;
    this.apiError = '';

    if (text.trim().length >= 2) {
      this.search$.next(text.trim());
    } else {
      this.suggestions = [];
      this.lastSearched = false;
    }
  }

  onKeydown(event: KeyboardEvent): void {
    if (!this.suggestionsOpen || !this.suggestions.length) return;

    if (event.key === 'ArrowDown') {
      event.preventDefault();
      this.activeIndex = Math.min(this.activeIndex + 1, this.suggestions.length - 1);
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      this.activeIndex = Math.max(this.activeIndex - 1, 0);
    } else if (event.key === 'Enter' && this.activeIndex >= 0) {
      event.preventDefault();
      this.applySuggestion(this.suggestions[this.activeIndex]);
    } else if (event.key === 'Escape') {
      this.suggestionsOpen = false;
    }
  }

  selectSuggestion(s: PlaceSuggestion, event: MouseEvent): void {
    event.preventDefault();
    this.applySuggestion(s);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.el.nativeElement.contains(event.target as Node)) {
      this.suggestionsOpen = false;
    }
  }

  private applySuggestion(s: PlaceSuggestion): void {
    const text = s.description?.trim() || s.mainText?.trim() || '';
    if (!text) return;

    this.value = text;
    this.valueChange.emit(text);
    this.addressInput.nativeElement.value = text;
    this.suggestions = [];
    this.suggestionsOpen = false;
    this.apiError = '';
    this.setValidated(true);
  }

  private setValidated(valid: boolean): void {
    if (this.validated === valid) return;
    this.validated = valid;
    this.validatedChange.emit(valid);
  }
}
