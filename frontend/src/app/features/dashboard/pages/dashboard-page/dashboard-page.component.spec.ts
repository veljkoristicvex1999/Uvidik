import { TestBed } from '@angular/core/testing';
import { DashboardPageComponent } from './dashboard-page.component';

describe('DashboardPageComponent', () => {
  it('renders the Serbian Latin placeholder heading', async () => {
    await TestBed.configureTestingModule({ imports: [DashboardPageComponent] }).compileComponents();
    const fixture = TestBed.createComponent(DashboardPageComponent);
    fixture.detectChanges();
    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Pregled');
    expect(text).toContain('U pripremi');
  });
});
