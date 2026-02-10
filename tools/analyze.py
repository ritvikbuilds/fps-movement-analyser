#!/usr/bin/env python3
"""
NoteD CSV Analyzer - Generate statistics and visualizations from input logs.
"""

import argparse
import csv
import os
from dataclasses import dataclass
from typing import Optional

@dataclass
class ClickStats:
    total_clicks: int = 0
    clicks_with_deadzone: int = 0
    clicks_with_counterstrafe: int = 0
    avg_deadzone_ms: float = 0.0
    avg_counterstrafe_ms: float = 0.0
    min_deadzone_ms: float = float('inf')
    max_deadzone_ms: float = 0.0
    min_counterstrafe_ms: float = float('inf')
    max_counterstrafe_ms: float = 0.0


def load_csv(filepath: str) -> list[dict]:
    """Load events from NoteD CSV file."""
    events = []
    with open(filepath, 'r', newline='') as f:
        reader = csv.DictReader(f)
        for row in reader:
            events.append({
                'timestamp_qpc': int(row['timestamp_qpc']),
                'timestamp_ms': float(row['timestamp_ms']),
                'device': row['device'],
                'key': row['key'],
                'event_type': row['event_type'],
                'deadzone_delta_ms': float(row['deadzone_delta_ms']) if row['deadzone_delta_ms'] else None,
                'counter_delta_ms': float(row['counter_delta_ms']) if row['counter_delta_ms'] else None,
            })
    return events


def compute_statistics(events: list[dict]) -> ClickStats:
    """Compute statistics from events."""
    stats = ClickStats()
    
    deadzone_values = []
    counterstrafe_values = []
    
    for evt in events:
        if evt['device'] == 'mouse' and evt['event_type'] == 'down':
            stats.total_clicks += 1
            
            if evt['deadzone_delta_ms'] is not None:
                stats.clicks_with_deadzone += 1
                deadzone_values.append(evt['deadzone_delta_ms'])
                stats.min_deadzone_ms = min(stats.min_deadzone_ms, evt['deadzone_delta_ms'])
                stats.max_deadzone_ms = max(stats.max_deadzone_ms, evt['deadzone_delta_ms'])
            
            if evt['counter_delta_ms'] is not None:
                stats.clicks_with_counterstrafe += 1
                counterstrafe_values.append(evt['counter_delta_ms'])
                stats.min_counterstrafe_ms = min(stats.min_counterstrafe_ms, evt['counter_delta_ms'])
                stats.max_counterstrafe_ms = max(stats.max_counterstrafe_ms, evt['counter_delta_ms'])
    
    if deadzone_values:
        stats.avg_deadzone_ms = sum(deadzone_values) / len(deadzone_values)
    if counterstrafe_values:
        stats.avg_counterstrafe_ms = sum(counterstrafe_values) / len(counterstrafe_values)
    
    if stats.min_deadzone_ms == float('inf'):
        stats.min_deadzone_ms = 0.0
    if stats.min_counterstrafe_ms == float('inf'):
        stats.min_counterstrafe_ms = 0.0
    
    return stats


def print_statistics(stats: ClickStats):
    """Print statistics to console."""
    print("\n=== NoteD Session Statistics ===\n")
    print(f"Total Clicks: {stats.total_clicks}")
    print(f"Clicks with Deadzone Data: {stats.clicks_with_deadzone} ({100*stats.clicks_with_deadzone/max(1,stats.total_clicks):.1f}%)")
    print(f"Clicks with Counterstrafe Data: {stats.clicks_with_counterstrafe} ({100*stats.clicks_with_counterstrafe/max(1,stats.total_clicks):.1f}%)")
    
    if stats.clicks_with_deadzone > 0:
        print(f"\n--- Deadzone Timing ---")
        print(f"Average: {stats.avg_deadzone_ms:.1f} ms")
        print(f"Min: {stats.min_deadzone_ms:.1f} ms")
        print(f"Max: {stats.max_deadzone_ms:.1f} ms")
    
    if stats.clicks_with_counterstrafe > 0:
        print(f"\n--- Counterstrafe Timing ---")
        print(f"Average: {stats.avg_counterstrafe_ms:.1f} ms")
        print(f"Min: {stats.min_counterstrafe_ms:.1f} ms")
        print(f"Max: {stats.max_counterstrafe_ms:.1f} ms")


def generate_timeline_png(events: list[dict], output_path: str):
    """Generate a PNG timeline visualization."""
    try:
        import matplotlib.pyplot as plt
        import matplotlib.patches as patches
    except ImportError:
        print("matplotlib not installed. Run: pip install matplotlib")
        return
    
    fig, ax = plt.subplots(figsize=(14, 4))
    ax.set_facecolor('#1A1A2E')
    fig.patch.set_facecolor('#1A1A2E')
    
    if not events:
        print("No events to visualize")
        return
    
    start_ms = events[0]['timestamp_ms']
    end_ms = events[-1]['timestamp_ms']
    duration_s = (end_ms - start_ms) / 1000
    
    key_colors = {'A': '#FF6B35', 'D': '#4ECDC4', 'W': '#FFD93D', 'S': '#6BCB77'}
    key_lanes = {'A': 0.6, 'D': 0.3}
    
    key_down_times = {}
    
    for evt in events:
        t = (evt['timestamp_ms'] - start_ms) / 1000
        
        if evt['device'] == 'keyboard':
            key = evt['key']
            if key in key_lanes:
                if evt['event_type'] == 'down':
                    key_down_times[key] = t
                elif evt['event_type'] == 'up' and key in key_down_times:
                    start_t = key_down_times[key]
                    rect = patches.Rectangle(
                        (start_t, key_lanes[key] - 0.1),
                        t - start_t, 0.2,
                        facecolor=key_colors.get(key, '#888888'),
                        alpha=0.8
                    )
                    ax.add_patch(rect)
                    del key_down_times[key]
        
        elif evt['device'] == 'mouse' and evt['event_type'] == 'down':
            ax.axvline(x=t, color='#FF3366', linewidth=1.5, alpha=0.7)
            ax.plot(t, 0.45, 'o', color='#FF3366', markersize=8)
            
            if evt['deadzone_delta_ms'] is not None:
                ax.annotate(f"{evt['deadzone_delta_ms']:.1f}",
                           (t, 0.5), color='white', fontsize=8,
                           ha='center', va='bottom',
                           bbox=dict(boxstyle='round,pad=0.2', facecolor='#1E1E3F', alpha=0.9))
    
    ax.set_xlim(0, duration_s)
    ax.set_ylim(0, 1)
    ax.set_xlabel('Time (seconds)', color='white')
    ax.tick_params(colors='white')
    ax.spines['bottom'].set_color('#2A2A4A')
    ax.spines['left'].set_visible(False)
    ax.spines['top'].set_visible(False)
    ax.spines['right'].set_visible(False)
    
    ax.text(-0.02, key_lanes['A'], 'A', color=key_colors['A'], fontsize=12, fontweight='bold',
            ha='right', va='center', transform=ax.get_yaxis_transform())
    ax.text(-0.02, key_lanes['D'], 'D', color=key_colors['D'], fontsize=12, fontweight='bold',
            ha='right', va='center', transform=ax.get_yaxis_transform())
    
    plt.tight_layout()
    plt.savefig(output_path, dpi=150, facecolor='#1A1A2E')
    plt.close()
    print(f"Timeline saved to: {output_path}")


def main():
    parser = argparse.ArgumentParser(description='Analyze NoteD CSV log files')
    parser.add_argument('csvfile', help='Path to NoteD CSV file')
    parser.add_argument('--png', help='Output PNG timeline path', default=None)
    parser.add_argument('--stats-only', action='store_true', help='Only print statistics')
    args = parser.parse_args()
    
    if not os.path.exists(args.csvfile):
        print(f"Error: File not found: {args.csvfile}")
        return 1
    
    events = load_csv(args.csvfile)
    print(f"Loaded {len(events)} events from {args.csvfile}")
    
    stats = compute_statistics(events)
    print_statistics(stats)
    
    if not args.stats_only and args.png:
        generate_timeline_png(events, args.png)
    elif not args.stats_only:
        png_path = args.csvfile.replace('.csv', '_timeline.png')
        generate_timeline_png(events, png_path)
    
    return 0


if __name__ == '__main__':
    exit(main())
