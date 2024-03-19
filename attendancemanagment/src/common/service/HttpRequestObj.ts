import {BaseContainer} from '../BaseContainer';
export class HttpRequestObj {
  public rqBody : BaseContainer;
  public rqTrack : BaseContainer;

  constructor(rqObj: BaseContainer) {
        this.rqBody = rqObj;
    }
}
