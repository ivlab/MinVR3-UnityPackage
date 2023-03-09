using System.Collections.Generic;

namespace IVLab.MinVR3
{

    public interface IVREventFilter
    {
        /// <summary>
        /// Each frame, IVREventFilters can process events and optionally modify them before the events
        /// are sent to IVREventListeners.  The VREventManager passes each event through the active filters
        /// just before that event is to be sent out to listeners.  A filter can then decide what to do
        /// with the event.  For example, an EventAlias filter could change the name of the event.  A
        /// ProximityEvent filter could listen for the position of two trackers and when they are within
        /// some threshold, generate a new event in response.  A filter could also completely discard the
        /// event.  the
        /// event from view of the
        /// IVREventListeners.  
        /// </summary>
        /// <param name="e">One event at a time is passed into the filter</param>
        /// <param name="filterResult">If the filter modifies the event in some way, this list returns the
        /// result of those modifications.  This parameter is a list rather than a single event since some
        /// filters may create a new event in response to the event and return both the original and the new
        /// events.  This will essentially "insert" a new event in the event queue as it is processed.</param>
        /// <returns><see langword="true"/> if the filter has modified the event in some way and returned the
        /// result in filterResult.  <see langword="false"/>if the event passes through the filter
        /// without change.</returns>
        bool FilterEvent(VREvent e, ref List<VREvent> filterResult);
    }

} // end namespace
