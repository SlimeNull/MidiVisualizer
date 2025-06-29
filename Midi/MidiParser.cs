using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Midi;

namespace Midi
{
    public class MidiParser
    {
        public class MidiData
        {
            public List<Note> Notes { get; set; } = new List<Note>();
            public double BeatsPerMinute { get; set; } = 120.0;
            public int DeltaTicksPerQuarterNote { get; set; }
        }

        public static MidiData ParseMidiFile(string filePath)
        {
            var midiFile = new MidiFile(filePath);
            var result = new MidiData
            {
                BeatsPerMinute = ExtractBpm(midiFile),
                DeltaTicksPerQuarterNote = midiFile.DeltaTicksPerQuarterNote
            };

            result.Notes = ExtractNotes(midiFile);
            return result;
        }

        private static List<Note> ExtractNotes(MidiFile midiFile)
        {
            var notes = new List<Note>();

            for (int trackIndex = 0; trackIndex < midiFile.Tracks; trackIndex++)
            {
                foreach (var midiEvent in midiFile.Events[trackIndex])
                {
                    if (midiEvent is NoteOnEvent noteOnEvent && noteOnEvent.Velocity > 0)
                    {
                        if (noteOnEvent.OffEvent != null)
                        {
                            notes.Add(new Note(
                                noteOnEvent.AbsoluteTime,
                                noteOnEvent.OffEvent.AbsoluteTime,
                                noteOnEvent.NoteNumber,
                                noteOnEvent.NoteName));
                        }
                        else
                        {
                            Console.WriteLine($"警告: 音符 {noteOnEvent.NoteName} 没有找到对应的结束事件");
                        }
                    }
                }
            }

            return notes;
        }

        private static double ExtractBpm(MidiFile midiFile)
        {
            for (int trackIndex = 0; trackIndex < midiFile.Tracks; trackIndex++)
            {
                foreach (var midiEvent in midiFile.Events[trackIndex])
                {
                    if (midiEvent is TempoEvent tempoEvent)
                    {
                        return Math.Round(60000000.0 / tempoEvent.MicrosecondsPerQuarterNote, 3);
                    }
                }
            }
            return 120.0; // 默认BPM
        }
    }

}
