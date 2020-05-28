import React, { Component } from 'react';
import { RoomHubEvents } from '../RoomConstants';
import styles from './timer.module.css';

export default class Timer extends Component {
    constructor(props) {
        super(props);
        this.state = { 
            time: { 
                minutesStr: '00',
                secondsStr: '00' 
            },
            seconds: 0 
        };
        this.timer = 0;
    }

    async componentDidMount() {
        this.props.hubConnection.on(RoomHubEvents.TIME_LEFT, (seconds) => {
            this.setState({
                seconds,
                time: this.secondsToTime(seconds)
            });
            this.startTimer();
        });

        await this.props.hubConnection.invoke(RoomHubEvents.REQUEST_TIME, this.props.roomId);
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
            minutesStr: minutes.toString().padStart(2, '0'),
            secondsStr: seconds.toString().padStart(2, '0')
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
            <div className={styles.timer}>
                {this.state.time.minutesStr} : {this.state.time.secondsStr}
            </div>
        );
    }
}