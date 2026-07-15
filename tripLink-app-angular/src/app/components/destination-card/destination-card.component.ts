import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Destination } from '../../models/models';

@Component({
  selector: 'app-destination-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <article
      class="dest-card"
      [class.selected]="selected"
      (click)="select.emit(destination)"
      tabindex="0"
      (keyup.enter)="select.emit(destination)">
      <div class="image-wrap">
        <img
          *ngIf="destination.imageUrl && !imgFailed"
          [src]="destination.imageUrl"
          [alt]="destination.nameDes"
          class="dest-image"
          (error)="onImgError()" />
        <div *ngIf="!destination.imageUrl || imgFailed" class="image-placeholder">
          <span class="placeholder-icon">🏞️</span>
        </div>
        <span class="region-badge">{{ destination.region }}</span>
      </div>

      <div class="card-body">
        <h3 class="dest-name">{{ destination.nameDes }}</h3>

        <div class="categories" *ngIf="destination.categories?.length">
          <span class="category-tag" *ngFor="let cat of destination.categories">{{ cat }}</span>
        </div>
        <p *ngIf="!destination.categories?.length" class="no-category">ללא קטגוריה</p>

        <div class="meta" *ngIf="destination.levelType || visitMinutes">
          <span *ngIf="destination.levelType" class="meta-chip">{{ destination.levelType }}</span>
          <span *ngIf="visitMinutes" class="meta-chip">{{ visitMinutes }} דקות ביקור</span>
        </div>
      </div>
    </article>
  `,
  styles: [`
    .dest-card {
      background: var(--tl-bg-card);
      border-radius: var(--tl-radius-lg);
      overflow: hidden;
      box-shadow: var(--tl-shadow-sm);
      cursor: pointer;
      transition: transform var(--tl-transition), box-shadow var(--tl-transition);
      border: 2px solid transparent;
      display: flex;
      flex-direction: column;
      height: 100%;
    }
    .dest-card:hover {
      transform: translateY(-4px);
      box-shadow: var(--tl-shadow-lg);
    }
    .dest-card.selected {
      border-color: var(--tl-lavender);
      box-shadow: var(--tl-shadow-lg), var(--tl-focus-ring);
    }
    .image-wrap {
      position: relative;
      height: 180px;
      background: var(--tl-gradient-subtle);
    }
    .dest-image {
      width: 100%;
      height: 100%;
      object-fit: cover;
      display: block;
    }
    .image-placeholder {
      width: 100%;
      height: 100%;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .placeholder-icon { font-size: 3rem; opacity: 0.7; }
    .region-badge {
      position: absolute;
      top: 12px;
      right: 12px;
      background: rgba(30, 27, 46, 0.72);
      color: var(--tl-text-inverse);
      padding: 4px 12px;
      border-radius: var(--tl-radius-pill);
      font-size: var(--tl-text-xs);
      font-weight: var(--tl-weight-semibold);
    }
    .card-body {
      padding: var(--tl-space-md);
      display: flex;
      flex-direction: column;
      gap: 10px;
      flex: 1;
    }
    .dest-name {
      margin: 0;
      font-size: var(--tl-text-lg);
      color: var(--tl-text);
      line-height: var(--tl-leading-tight);
    }
    .categories {
      display: flex;
      flex-wrap: wrap;
      gap: 6px;
    }
    .category-tag {
      background: var(--tl-gradient-subtle);
      color: var(--tl-lavender-deep);
      padding: 4px 12px;
      border-radius: var(--tl-radius-pill);
      font-size: var(--tl-text-xs);
      font-weight: var(--tl-weight-semibold);
    }
    .no-category {
      margin: 0;
      color: var(--tl-text-muted);
      font-size: var(--tl-text-sm);
    }
    .meta {
      display: flex;
      flex-wrap: wrap;
      gap: 6px;
      margin-top: auto;
    }
    .meta-chip {
      background: var(--tl-bg-muted);
      color: var(--tl-text-secondary);
      padding: 3px 10px;
      border-radius: var(--tl-radius-sm);
      font-size: var(--tl-text-xs);
    }
  `]
})
export class DestinationCardComponent implements OnChanges {
  @Input({ required: true }) destination!: Destination;
  @Input() selected = false;
  @Output() select = new EventEmitter<Destination>();

  imgFailed = false;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['destination']) this.imgFailed = false;
  }

  get visitMinutes(): number | null {
    const t = this.destination.timeDes;
    if (t == null) return null;
    if (typeof t === 'number') return t;
    const parts = String(t).split(':').map(Number);
    if (parts.length >= 2 && !Number.isNaN(parts[0]) && !Number.isNaN(parts[1]))
      return parts[0] * 60 + parts[1];
    return null;
  }

  onImgError(): void {
    this.imgFailed = true;
  }
}
