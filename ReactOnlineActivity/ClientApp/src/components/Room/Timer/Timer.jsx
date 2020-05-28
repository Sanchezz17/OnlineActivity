import React, { Component } from 'react';
import { RoomHubEvents } from '../RoomConstants';

export default class Timer extends Component {
    constructor(props) {
        super(props);
        this.state = { time: {minutes: 0, seconds: 0}, seconds: 0 };
        this.timer = 0;
    }

    async componentDidMount() {
        await this.props.hubConnection.invoke(RoomHubEvents.REQUEST_TIME, this.props.roomId);
        this.props.hubConnection.on(RoomHubEvents.TIME_LEFT, (seconds) => {
            this.setState({
                seconds,
                time: this.secondsToTime(seconds)
            });
            this.startTimer();
        });
    }

    startTimer = () => {
        if (this.timer === 0 && this.state.seconds > 0) {
            this.timer = setInterval(this.countDown, 1000);
        }
    };

    secondsToTime = (totalSeconds) => {
        const divisorForMinutes = totalSeconds % (60 * 60);
        const minutes = Math.floor(divisorForMinutes / 60);
        const divisorForSeconds = divisorForMinutes % 60;
        const seconds = Math.ceil(divisorForSeconds);

        return {
            minutes,
            seconds
        };
    };
    
    countDown = () => {
        let seconds = this.state.seconds - 1;
        this.setState({
            time: this.secondsToTime(seconds),
            seconds: seconds,
        });

        if (seconds === 0) {
            clearInterval(this.timer);
        }
    };

    render() {
        return(
            <div>
                {this.state.time.minutes} : {this.state.time.seconds}
            </div>
        );
    }
}